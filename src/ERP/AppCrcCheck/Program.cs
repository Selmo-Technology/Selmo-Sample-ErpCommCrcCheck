using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        // prepare recipe data
        var myRecipe = new MyRecipe()
        {
            iRecipeValue1 = 1,
            iRecipeValue8 = 2,

            rRecipeValue2 = 3.123f,
            rRecipeValue4 = 4.123f,
            rRecipeValue5 = 5.123f,
            rRecipeValue6 = 6.123f,
            rRecipeValue7 = 7.123f,

            bRecipeFlag1 = true,
            bRecipeFlag2 = false
        };

        // write recipe data to PLC with any data provider like OPC UA
        // TODO: plc.WriteAny(myRecipe);

        // Calculate CRC
        var definition = new Nito.HashAlgorithms.CRC32.Definition
        {
            Initializer = 0xFFFFFFFF,
            TruncatedPolynomial = 0x04C11DB7,
            FinalXorValue = 0x00000000,
            ReverseResultBeforeFinalXor = true,
            ReverseDataBytes = true
        };

        // Get byte array for myRecipe data
        var bytes = JsonSerializer.SerializeToUtf8Bytes(myRecipe);

        // initializes the CRC library
        var crcAlgorithm = new Nito.HashAlgorithms.CRC32(definition);

        // calculate the CRC
        byte[] crc = crcAlgorithm.ComputeHash(bytes);

        // Write CRC to PLC
        uint dwCrc_ERP = BitConverter.ToUInt32(crc, 0);
        // TODO: plc.WriteAny(dwCrc_ERP);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MyRecipe
    {
        [MarshalAs(UnmanagedType.I2)]
        public short iRecipeValue1;

        [MarshalAs(UnmanagedType.I2)]
        public short iRecipeValue8;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue2;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue3;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue4;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue5;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue6;

        [MarshalAs(UnmanagedType.R4)]
        public float rRecipeValue7;

        [MarshalAs(UnmanagedType.Bool)]
        public bool bRecipeFlag1;

        [MarshalAs(UnmanagedType.Bool)]
        public bool bRecipeFlag2;
    }
}