# AutoByte

[![.NET](https://github.com/Jandini/AutoByte/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/AutoByte/actions/workflows/nuget.yml)

Data structure deserializer



## Quick Start

The sample data structure consists of two bytes and text with a length stored in the first two bytes.

```c#
Offset      0  1  2  3  4  5  6  7   8  9 10 11 12 13 14 15

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
        Text = slide.GetUTF8String(Length);
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

    const int EXCPECTED_LENGTH = 126;
    const string EXPECTED_TEXT = "MIT License\r\n\r\nCopyright (c) 2023 Matt Janda\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy\r\n";

    Assert.Equal(EXCPECTED_LENGTH, license.Length);
    Assert.Equal(EXPECTED_TEXT, license.Text);       
}
```







### Resources

Byte icon was downloaded from [Flaticon](https://www.flaticon.com/free-icon/byte_5044438)


