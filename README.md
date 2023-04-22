# AutoByte

[![.NET](https://github.com/Jandini/AutoByte/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml)


Fast .NET data structure deserializer and parser. 

* Automatically generate implementation from the class properties.

* Provides `ByteSlide` parser.

* Use `BinaryPrimitives` and `ReadOnlySpan<byte>` to read the data. 

  

## Quick Start

A few examples how to use AutoByte. 

### Zip Header

The [zip header](https://en.wikipedia.org/wiki/ZIP_(file_format)) is well documented. 

| Offset | Bytes | Description                                                  |
| ------ | ----- | ------------------------------------------------------------ |
| 0      | 4     | Local file header signature = 0x04034b50 (PK♥♦ or "PK\3\4")  |
| 4      | 2     | Version needed to extract (minimum)                          |
| 6      | 2     | General purpose bit flag                                     |
| 8      | 2     | Compression method; e.g. none = 0, DEFLATE = 8 (or "\0x08\0x00") |
| 10     | 2     | File last modification time                                  |
| 12     | 2     | File last modification date                                  |
| 14     | 4     | CRC-32 of uncompressed data                                  |
| 18     | 4     | Compressed size (or 0xffffffff for ZIP64)                    |
| 22     | 4     | Uncompressed size (or 0xffffffff for ZIP64)                  |
| 26     | 2     | File name length (*n*)                                       |
| 28     | 2     | Extra field length (*m*)                                     |
| 30     | *n*   | File name                                                    |
| 30+*n* | *m*   | Extra field                                                  |

The zip header in hex editor looks like this.

```c#
Offset      0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F

00000000   50 4B 03 04 14 00 00 00  08 00 63 54 96 56 45 7F   PK        cT–VE 
00000010   6A BD 5B 02 00 00 F4 08  00 00 0C 00 00 00 41 75   j½[   ô       Au
00000020   74 6F 42 79 74 65 2E 73  6C 6E                     toByte.sln
```

The same zip header in C# byte array.

```c#
var header = new byte[] 
{
    0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54, 
    0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08, 
    0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79, 
    0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
};
```

Here is how to use [AutoByte](https://github.com/Jandini/AutoByte) to parse zip file header.  

1. Create a new class.

    ```c#
    public class ZipFileHeader
    {    
    }
    ```

2. Decorate the class with `AutoByteStructure` and make it `partial` to activate code generator.

    ```c#
    [AutoByteStructure]
    public partial class ZipFileHeader
    {    
    }
    ```

3. Add zip header properties based on the documentation. 

    ```c#
    [AutoByteStructure]
    public partial class ZipFileHeader
    {
        public int Signature { get; set; }
        public short VersionNeededToExtract { get; set; }
        public short GeneralPurposeBitFlag { get; set; }
        public short CompressionMethod { get; set; }
        public short LastModifiedFileTime { get; set; }
        public short LastModifiedFileDate { get; set; }
        public int Crc32 { get; set; }
        public int CompressedSize { get; set; }
        public int UncompressedSize { get; set; }
        public short FileNameLength { get; set; }
        public short ExtraFieldLength { get; set; }
        public string FileName { get; set; }
        public byte[] ExtraField { get; set; }   
    }
    ```



4. Non primitive types like arrays or strings must be decorated with `AutoByteField` or `AutoByteString` attribute.  Use `SizeFromProperty` to get the size from previously parsed properties.

    ```c#
    [AutoByteStructure]
    public partial class ZipFileHeader
    {
        public int Signature { get; set; }
        public short VersionNeededToExtract { get; set; }
        public short GeneralPurposeBitFlag { get; set; }
        public short CompressionMethod { get; set; }
        public short LastModifiedFileTime { get; set; }
        public short LastModifiedFileDate { get; set; }
        public int Crc32 { get; set; }
        public int CompressedSize { get; set; }
        public int UncompressedSize { get; set; }
        public short FileNameLength { get; set; }
        public short ExtraFieldLength { get; set; }
    
        [AutoByteString(SizeFromProperty = "FileNameLength")]
        public string FileName { get; set; }    
    
        [AutoByteField(SizeFromProperty = "ExtraFieldLength")]
        public byte[] ExtraField { get; set; }
    }
    ```



5. Get zip header with `ByteSlide` structure.

    ```c#
    // zip header in byte array
    var data = new byte[]
    {
        0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54,
        0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08,
        0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79,
        0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
    };
    
    // create byte slide from byte array
    var header = new ByteSlide(data);
    
    // deserialize the zip header structure
    var zip = header.GetStructure<ZipFileHeader>();
    ```



The code generator will create the implementation for you.

```c#
public partial class ZipFileHeader : IByteStructure
{
    public static int StructureSize = 30;
    public int Deserialize(ref ByteSlide slide)
    {
        Signature = slide.GetInt32LittleEndian();
        VersionNeededToExtract = slide.GetInt16LittleEndian();
        GeneralPurposeBitFlag = slide.GetInt16LittleEndian();
        CompressionMethod = slide.GetInt16LittleEndian();
        LastModifiedFileTime = slide.GetInt16LittleEndian();
        LastModifiedFileDate = slide.GetInt16LittleEndian();
        Crc32 = slide.GetInt32LittleEndian();
        CompressedSize = slide.GetInt32LittleEndian();
        UncompressedSize = slide.GetInt32LittleEndian();
        FileNameLength = slide.GetInt16LittleEndian();
        ExtraFieldLength = slide.GetInt16LittleEndian();
        FileName = slide.GetUtf8String(FileNameLength);
        ExtraField = slide.Slide(ExtraFieldLength).ToArray();
        return 0;
    }
}
```



### Master Boot Record

Following example demonstrate how read [Master Boot Record](https://en.wikipedia.org/wiki/Master_boot_record) with AutoByte. 

1. Create `PartitionType` enum and set the size to 1 `byte`.

    ```c#
    public enum PartitionType : byte
    {
        Empty = 0x00,
        Fat12 = 0x01,
        XENIXRoot = 0x02,
        XENIXUser = 0x03,
        FAT16 = 0x04,
        Extended = 0x05,
        FAT32 = 0x0B,
        FAT32LBA = 0x0C,
        Linux = 0x83,
    }
    ```

2. Create `PartitionEntry` class.

    ```c#
    [AutoByteStructure]
    public partial class PartitionEntry
    {
        public byte BootIndicator { get; set; }
        [AutoByteField(Size = 3)]
        public byte[] StartingCHS { get; set; }
        public PartitionType Type { get; set; }
        [AutoByteField(Size = 3)]
        public byte[] EndingCHS { get; set; }
        public uint StartingLBA { get; set; }
        public uint SizeInLBA { get; set; }
    }
    ```

3. Create non-partial `MasterBootRecord` class with `IByteStructure` interface. 

    ```c#
    public class MasterBootRecord : IByteStructure
    {
        public byte[] BootCode { get; private set; }
        public PartitionEntry[] PartitionEntries { get; private set; }
        public ushort BootSignature { get; private set; }

        public int Deserialize(ref ByteSlide slide)
        {
            // get boot code 
            BootCode = slide.GetByteArray(446);

            // read all 4 partition entries
            PartitionEntries = new PartitionEntry[4];
            for (int i = 0; i < 4; i++)
                PartitionEntries[i] = slide.GetStructure<PartitionEntry>();

            // read signature (0xAA55)
            BootSignature = slide.GetUInt16LittleEndian();
            return 0;
        }
    }
    ```

4. Read the structure with `ByteSlide` structure.

    ```c#
    // read first sector (512 bytes) from first physical drive (windows only)
    using var drive = File.OpenRead("//?/PHYSICALDRIVE0");
    var sector = new byte[512];
    drive.Read(sector);
    
    // create byte slide and read master boot record
    var slide = new ByteSlide(sector);
    var mbr = slide.GetStructure<MasterBootRecord>();
    ```




### UTF8 Text 

The sample data structure consists of two bytes and text with a length stored in the first two bytes.

```c#
Offset      0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F

00000000   7E 00 4D 49 54 20 4C 69  63 65 6E 73 65 0D 0A 0D   ~ MIT License   
00000016   0A 43 6F 70 79 72 69 67  68 74 20 28 63 29 20 32    Copyright (c) 2
00000032   30 32 33 20 4D 61 74 74  20 4A 61 6E 64 61 0D 0A   023 Matt Janda  
00000048   0D 0A 50 65 72 6D 69 73  73 69 6F 6E 20 69 73 20     Permission is 
00000064   68 65 72 65 62 79 20 67  72 61 6E 74 65 64 2C 20   hereby granted, 
00000080   66 72 65 65 20 6F 66 20  63 68 61 72 67 65 2C 20   free of charge, 
00000096   74 6F 20 61 6E 79 20 70  65 72 73 6F 6E 20 6F 62   to any person ob
00000112   74 61 69 6E 69 6E 67 20  61 20 63 6F 70 79 0D 0A   taining a copy  
```



1. Create a class for the license object.

    ```c#
    public class License 
    {
    }
    ```

1. Add `IByteStructure` interface from AutoByte.

    ```c#
    public class License : IByteStructure
    {
        public int Deserialize(ref ByteSlide slide)
        {
            return 0;
        }
    }
    ```
    
1. Add structure properties. 

    ```c#
    public class License : IByteStructure
    {
        public short Length { get; set; }
        public string Text { get; set; }   
    
        public int Deserialize(ref ByteSlide slide)
        {
            return 0;
        }
    }
    ```
    
    
    
1. Use `ByteSlide` to read data.

    ```c#
    public class License : IByteStructure
    {
        public short Length { get; set; }
        public string Text { get; set; }   
    
        public int Deserialize(ref ByteSlide slide)
        {
            Length = slide.GetInt16LittleEndian();
            Text = slide.GetUtf8String(Length);
            return 0;
        }
    }
    ```



The entire test code can look like this. 

```c#
[Fact]
public void Test1()
{
    var data = new byte[] {
    	0x7E, 0x00, 0x4D, 0x49, 0x54, 0x20, 0x4C, 0x69, 0x63, 0x65, 0x6E, 0x73, 0x65,             
        0x0D, 0x0A, 0x0D, 0x0A, 0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74,
    	0x20, 0x28, 0x63, 0x29, 0x20, 0x32, 0x30, 0x32, 0x33, 0x20, 0x4D, 0x61, 0x74, 
    	0x74, 0x20, 0x4A, 0x61, 0x6E, 0x64, 0x61, 0x0D, 0x0A, 0x0D, 0x0A, 0x50, 0x65, 
    	0x72, 0x6D, 0x69, 0x73, 0x73, 0x69, 0x6F, 0x6E, 0x20, 0x69, 0x73, 0x20, 0x68, 
    	0x65, 0x72, 0x65, 0x62, 0x79, 0x20, 0x67, 0x72, 0x61, 0x6E, 0x74, 0x65, 0x64, 
    	0x2C, 0x20, 0x66, 0x72, 0x65, 0x65, 0x20, 0x6F, 0x66, 0x20, 0x63, 0x68, 0x61, 
    	0x72, 0x67, 0x65, 0x2C, 0x20, 0x74, 0x6F, 0x20, 0x61, 0x6E, 0x79, 0x20, 0x70, 
    	0x65, 0x72, 0x73, 0x6F, 0x6E, 0x20, 0x6F, 0x62, 0x74, 0x61, 0x69, 0x6E, 0x69, 
    	0x6E, 0x67, 0x20, 0x61, 0x20, 0x63, 0x6F, 0x70, 0x79, 0x0D, 0x0A
    };

    var slide = new ByteSlide(data);
    var license = slide.GetStructure<LicenseText>();

    const int EXPECTED_LENGTH = 126;
    const string EXPECTED_TEXT = "MIT License\r\n\r\nCopyright (c) 2023 Matt Janda\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy\r\n";

    Assert.Equal(EXPECTED_LENGTH, license.Length);
    Assert.Equal(EXPECTED_TEXT, license.Text);       
}
```







### Resources

Byte icon was downloaded from [Flaticon](https://www.flaticon.com/free-icon/byte_5044438)

