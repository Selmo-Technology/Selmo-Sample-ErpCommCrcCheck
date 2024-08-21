## CRC Check for Recipe Data Integrity
A Cyclic Redundancy Check (CRC) is a widely used method for detecting errors in data transmission or storage. In the context of verifying recipe data received from an ERP system, the CRC function plays a crucial role in ensuring that the data has not been corrupted or altered during transfer.

**Process Overview:**

**1.) Generate CRC on ERP Side:**
* When the recipe data is generated or prepared by the ERP system, a CRC value is computed based on the entire dataset.
* This CRC value is then appended to the recipe data before transmission.

**2.) Receive and Extract Data:**
* The PLC receives the recipe data along with the appended CRC value.
* The recipe data and the CRC value are separated.

**3.) Recompute CRC on PLC Side:**
* The PLC computes its own CRC value using the received recipe data (excluding the received CRC value).
* This computed CRC value represents what the CRC should be if the data is intact.

**4.) Compare CRC Values:**
* The PLC compares the recomputed CRC value with the received CRC value.
* If the two CRC values match, it indicates that the recipe data is intact and has not been corrupted during transmission.
* If the CRC values do not match, it suggests that the data may have been altered or corrupted, and the recipe should be rejected or flagged for review.



### Example of computing CRC on ERP Side (.NET 8 sample program)
```
// Recipe struct
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
```
```
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
```


### Example of recomputing CRC on PLC Side
```
TYPE MyRecipe :
STRUCT
	iRecipeValue1 : INT := 123;
	iRecipeValue8 : INT := 345;
	
	rRecipeValue2 : REAL := 234.5232;
	rRecipeValue3 : REAL := 823764.326;
	rRecipeValue4 : REAL := 2343463.346;
	rRecipeValue5 : REAL := 6532.12;
	rRecipeValue6 : REAL := 6.23;
	rRecipeValue7 : REAL := 74.235;
	
    	bRecipeFlag1 : BOOL := TRUE;
	bRecipeFlag2 : BOOL := FALSE;
END_STRUCT
END_TYPE

```
```
PROGRAM MAIN
VAR
	// ######## ERP DATA ##################
	stRecipe_ERP	: MyRecipe;             	// The recipe data from the ERP system
	dwCrc_ERP	: DWORD;			// CRC32 which is calculated and written by ERP system
	
	// ######## LOCAL #####################
	byteArray	: ARRAY[0..255] OF BYTE;	// BYTE array to store the result
	dwChecksum	: DWORD;			// CRC32 Result
	dwCrc_ERP_cached: DWORD;			// CRC32 which is calculated and written by ERP system
	xCrcOK		: BOOL;				// Data is OK
END_VAR
```
```
// Wait for changes on ERP_CRC
IF dwCrc_ERP <> dwCrc_ERP_cached THEN
	dwCrc_ERP_cached := dwCrc_ERP;
	xCrcOK := FALSE;
	
	// Copy STRUCT to BYTE array
	MEMCPY(ADR(byteArray), ADR(stRecipe_ERP), SIZEOF(stRecipe_ERP));
	
	// CAlculate the CRC32
	dwChecksum := CRC_GEN(
		PT:= ADR (byteArray), 
		SIZE:= UINT_TO_INT(SIZEOF(stRecipe_ERP)), 
		PL:= 32, 
		PN:= 16#04C11DB7, 
		INIT:= 16#FFFFFFFF, 
		REV_IN:= TRUE, 
		REV_OUT:= TRUE, 
		XOR_OUT:= 0);

	// Check if CRC is OK
	IF dwCrc_ERP = dwChecksum THEN
		xCrcOK := TRUE;
	END_IF
END_IF
```
