# AutoByte

[![.NET](https://github.com/Jandini/AutoByte/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml)

.NET data structure deserializer and parser.  AutoByte use `BinaryPrimitives` and `ReadOnlySpan<byte>` byte spans to read the data. 



## Quick Start

Here is how to use [AutoByte](https://github.com/Jandini/AutoByte) to parse ZIP file header.  

* Create partial `ZipFileHeader` class.
* Decorate the class with `AutoByteStructure` attribute.
* Decorate `FileName` property with `AutoByteString` attribute.  Use `SizeFromProperty` to set the size to  `FileNameLength` value.
* Decorate `ExtraField` property with `AutoByteField` attribute. Use `SizeFromProperty` to set the size to `ExtraFieldLength` value. 

The class should look like this: 

```c#
[AutoByteStructure]
public partial class ZipFileHeader
{
    public int Signature { get; set; }
    public short VersionMadeBy { get; set; }
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

That is all. Now use `ByteSlide` to read the header: 

```c#
var data = new byte[]
{
    0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54,
    0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08,
    0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79,
    0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
};

var header = new ByteSlide(data);
var zip = header.GetStructure<ZipFileHeader>();
```



AutoByte's code generator will create the implementation for you:

```c#
public partial class ZipFileHeader : IByteStructure
{
    public static int StructureSize = 32;
    public int Deserialize(ref ByteSlide slide)
    {
        Signature = slide.GetInt32LittleEndian();
        VersionMadeBy = slide.GetInt16LittleEndian();
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
        return 32;
    }
}
```





### Step by step 

The example use a zip file header created from a single file "AutoByte.sln" file compressed into a zip archive. The [zip header](https://en.wikipedia.org/wiki/ZIP_(file_format)) is well documented. 

| Offset | Bytes | Description                              |
| ------ | ----- | ---------------------------------------- |
| 0      | 4     | Local file header signature = 0x04034b50 (PK♥♦ or "PK\3\4") |
| 4      | 2     | Version needed to extract (minimum)      |
| 6      | 2     | General purpose bit flag                 |
| 8      | 2     | Compression method; e.g. none = 0, DEFLATE = 8 (or "\0x08\0x00") |
| 10     | 2     | File last modification time              |
| 12     | 2     | File last modification date              |
| 14     | 4     | CRC-32 of uncompressed data              |
| 18     | 4     | Compressed size (or 0xffffffff for ZIP64) |
| 22     | 4     | Uncompressed size (or 0xffffffff for ZIP64) |
| 26     | 2     | File name length (*n*)                   |
| 28     | 2     | Extra field length (*m*)                 |
| 30     | *n*   | File name                                |
| 30+*n* | *m*   | Extra field                              |

The example zip header in the hex editor.

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



Create a C# class that represents zip file header based on the documentation.  

> Note that each property type represents byte size of the field; `byte ` is 1 byte,  ` short` is 2 bytes,  `int` is 4 bytes and `long` is 8 bytes. 

```c#
public class ZipFileHeader
{
    /// <summary>
    /// Signature for ZIP file header
    /// </summary>
    public int Signature { get; set; }

    /// <summary>
    /// Version of software used to create ZIP file
    /// </summary>
    public short VersionMadeBy { get; set; }

    /// <summary>
    /// Version of software needed to extract ZIP file
    /// </summary>
    public short VersionNeededToExtract { get; set; }

    /// <summary>
    /// General purpose bit flag
    /// </summary>
    public short GeneralPurposeBitFlag { get; set; }

    /// <summary>
    /// Compression method used
    /// </summary>
    public short CompressionMethod { get; set; }

    /// <summary>
    /// Last modified file time
    /// </summary>
    public short LastModifiedFileTime { get; set; }

    /// <summary>
    /// Last modified file date
    /// </summary>
    public short LastModifiedFileDate { get; set; }

    /// <summary>
    /// CRC-32 checksum of file
    /// </summary>
    public int Crc32 { get; set; }

    /// <summary>
    /// Compressed size of file
    /// </summary>
    public int CompressedSize { get; set; }

    /// <summary>
    /// Uncompressed size of file
    /// </summary>
    public int UncompressedSize { get; set; }

    /// <summary>
    /// Length of file name
    /// </summary>
    public short FileNameLength { get; set; }

    /// <summary>
    /// Length of extra field
    /// </summary>
    public short ExtraFieldLength { get; set; }

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Extra field
    /// </summary>
    public byte[] ExtraField { get; set; }       
}
```



Decorate the class with ` [AutoByteStructure]` attribute and make the class `partial`. The class must be marked as partial because the code generator will create deserialization code in separate file. 

 









### Without code generator

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



Create DTO class for the license object and apply IByteStructure implementation for Deserialize method using ByteSlide:

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



Use ByteSlide to deserialize data structure into DTO class:

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


