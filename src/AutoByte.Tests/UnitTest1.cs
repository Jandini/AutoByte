using AutoByte;

namespace AutByte.Tests
{
    public class UnitTest1
    {
        public class LicenseText : IByteStructure
        {
            public short Length { get; set; }
            public string? Text { get; set; }

            public int Deserialize(ref ByteSlide slide)
            {
                Length = slide.GetInt16LittleEndian();
                Text = slide.GetUTF8String(Length);
                return 0;
            }
        }

        [Fact]
        public void Test1()
        {
            var data = new byte[] {

                0x7E, 0x00, 0x4D, 0x49, 0x54, 0x20, 0x4C, 0x69, 0x63, 0x65, 0x6E, 0x73, 0x65, 0x0D, 0x0A, 0x0D,
                0x0A, 0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x20, 0x28, 0x63, 0x29, 0x20, 0x32,
                0x30, 0x32, 0x33, 0x20, 0x4D, 0x61, 0x74, 0x74, 0x20, 0x4A, 0x61, 0x6E, 0x64, 0x61, 0x0D, 0x0A,
                0x0D, 0x0A, 0x50, 0x65, 0x72, 0x6D, 0x69, 0x73, 0x73, 0x69, 0x6F, 0x6E, 0x20, 0x69, 0x73, 0x20,
                0x68, 0x65, 0x72, 0x65, 0x62, 0x79, 0x20, 0x67, 0x72, 0x61, 0x6E, 0x74, 0x65, 0x64, 0x2C, 0x20,
                0x66, 0x72, 0x65, 0x65, 0x20, 0x6F, 0x66, 0x20, 0x63, 0x68, 0x61, 0x72, 0x67, 0x65, 0x2C, 0x20,
                0x74, 0x6F, 0x20, 0x61, 0x6E, 0x79, 0x20, 0x70, 0x65, 0x72, 0x73, 0x6F, 0x6E, 0x20, 0x6F, 0x62,
                0x74, 0x61, 0x69, 0x6E, 0x69, 0x6E, 0x67, 0x20, 0x61, 0x20, 0x63, 0x6F, 0x70, 0x79, 0x0D, 0x0A
            };

            var slide = new ByteSlide(data);
            var license = slide.GetStructure<LicenseText>();

            const int EXCPECTED_LENGTH = 126;
            const string EXPECTED_TEXT = "MIT License\r\n\r\nCopyright (c) 2023 Matt Janda\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy\r\n";

            Assert.Equal(EXCPECTED_LENGTH, license.Length);
            Assert.Equal(EXPECTED_TEXT, license.Text);
        }
    }
}