using System.Buffers.Binary;
using System.Text;
using AutoByte.Tests.Structures;

namespace AutoByte.Tests
{
    public class ByteSlide_Must
    {
      
        [Fact]
        public void GetHfsDateUtcLittleEndian_ReturnsEpoch()
        {
            var HFS_DATE_EPOCH = new DateTime(1904, 1, 1);
            const int HFS_DATE_SIZE = 4;
            var buffer = new byte[HFS_DATE_SIZE];

            var slide = new ByteSlide(buffer);

            Assert.Equal(slide.GetHfsDateUtcLittleEndian(), HFS_DATE_EPOCH);
        }

        [Fact]
        public void GetHfsDateUtcBigEndian_ReturnsEpoch()
        {
            var HFS_DATE_EPOCH = new DateTime(1904, 1, 1);
            const int HFS_DATE_SIZE = 4;
            var buffer = new byte[HFS_DATE_SIZE];

            var slide = new ByteSlide(buffer);

            Assert.Equal(slide.GetHfsDateUtcBigEndian(), HFS_DATE_EPOCH);
        }


        [Fact]
        public void GetCDateUtcLittleEndian_ReturnsEpoch()
        {
            const int C_DATE_SIZE = 4;
            var C_DATE_EPOCH = new DateTime(1970, 1, 1);

            var buffer = new byte[C_DATE_SIZE];

            var slide = new ByteSlide(buffer);

            Assert.Equal(slide.GetCDateUtcLittleEndian(), C_DATE_EPOCH);
        }

        [Fact]
        public void GetCDateUtcBigEndian_ReturnsEpoch()
        {
            const int C_DATE_SIZE = 4;
            var C_DATE_EPOCH = new DateTime(1970, 1, 1);

            var buffer = new byte[C_DATE_SIZE];

            var slide = new ByteSlide(buffer);

            Assert.Equal(slide.GetCDateUtcBigEndian(), C_DATE_EPOCH);
        }


       



        [Fact]
        public void GetCDateUtcBigEndian_ReturnsExpectedDate()
        {
            DateTime EXPECTED_DATE_TIME = new(1980, 08, 21, 19, 45, 00);
            byte[] C_DATE_BUFFER = { 0x14, 0x02, 0xE9, 0x3C };

            var slide = new ByteSlide(C_DATE_BUFFER);

            Assert.Equal(slide.GetCDateUtcBigEndian(), EXPECTED_DATE_TIME);
        }

        [Fact]
        public void GetCDateUtcLittleEndian_ReturnsExpectedDate()
        {
            DateTime EXPECTED_DATE_TIME = new(2002, 05, 20, 14, 03, 00);
            byte[] C_DATE_BUFFER = { 0x14, 0x02, 0xE9, 0x3C };

            var slide = new ByteSlide(C_DATE_BUFFER);

            Assert.Equal(slide.GetCDateUtcLittleEndian(), EXPECTED_DATE_TIME);
        }




        [Fact]
        public void GetCString_ReturnsExpectedStirngAndCompleteSlide()
        {
            byte[] TWO_C_STRINGS = {
                0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00, 0x4D, 0x61, 0x74, 0x74,
                0x20, 0x4A, 0x61, 0x6E, 0x64, 0x61, 0x00
            };

            string EXPECTED_FIRST_TEXT = "Hello World";
            string EXPECTED_SECOND_TEXT = "Matt Janda";

            var slide = new ByteSlide(TWO_C_STRINGS);

            Assert.Equal(slide.GetCString(Encoding.ASCII), EXPECTED_FIRST_TEXT);
            Assert.Equal(slide.GetCString(Encoding.ASCII), EXPECTED_SECOND_TEXT);

            Assert.Equal(0, slide.Length);
        }


        [Fact]        
        public void GetCString_ThrowsUnterminatedCStringException()
        {            
            byte[] UNTERMINATED_STRING = {
                0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64
            };

            
            Assert.Throws<ByteSlideException>(() => {
                var slide = new ByteSlide(UNTERMINATED_STRING);
                slide.GetCString(Encoding.ASCII); 
            });
        }


        [Fact]
        public void GetPascalString_ReturnsExcpectedStrings()
        {
            byte[] PASCAL_STRINGS = {
                0x0B, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x0A, 0x4D, 0x61, 0x74,
                0x74, 0x20, 0x4A, 0x61, 0x6E, 0x64, 0x61, 0x00
            };

            var slide = new ByteSlide(PASCAL_STRINGS);
            
            Assert.Equal("Hello World", slide.GetPascalString(Encoding.ASCII));
            Assert.Equal("Matt Janda", slide.GetPascalString(Encoding.ASCII));
            Assert.Equal("", slide.GetPascalString(Encoding.ASCII));

            Assert.Equal(0, slide.Length);

        }

        /// <summary>
        /// Test that demonstrates the PeekStructure bug.
        /// 
        /// BUG: PeekStructure is supposed to be non-destructive (like Stack.Peek or Queue.Peek),
        /// but it advances the internal pointer due to passing `ref this` to Deserialize().
        /// 
        /// This test WILL FAIL with the current buggy implementation because:
        /// 1. PeekStructure calls Deserialize(ref this), which advances the pointer
        /// 2. GetStructure then reads from the ADVANCED position, not the original
        /// 3. The signatures will differ (peeked reads correct bytes, actual reads garbage)
        /// 
        /// The test demonstrates "silent data corruption" - no exception is thrown,
        /// the methods just return different results from the same data position.
        /// </summary>
        [Fact]
        public void PeekStructure_ShouldNotAdvancePointer_BUG()
        {
            // ZIP header data: 0x50 0x4B 0x03 0x04 (signature PK\x03\x04)
            var data = new byte[]
            {
                0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54,
                0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08,
                0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79,
                0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
            };

            var slide = new ByteSlide(data);

            // Peek at the structure (should NOT advance pointer)
            var peeked = slide.PeekStructure<ZipFileHeader>();

            // Now get the actual structure (should read from SAME position as peek)
            var actual = slide.GetStructure<ZipFileHeader>();

            // These should be IDENTICAL because peek should not advance the pointer
            // If they differ, it proves the bug: peek advanced the pointer
            Assert.Equal(peeked.Signature, actual.Signature);
            Assert.Equal(peeked.VersionNeededToExtract, actual.VersionNeededToExtract);
            Assert.Equal(peeked.GeneralPurposeBitFlag, actual.GeneralPurposeBitFlag);
            Assert.Equal(peeked.CompressionMethod, actual.CompressionMethod);
            Assert.Equal(peeked.LastModifiedFileTime, actual.LastModifiedFileTime);
            Assert.Equal(peeked.LastModifiedFileDate, actual.LastModifiedFileDate);
            Assert.Equal(peeked.Crc32, actual.Crc32);
            Assert.Equal(peeked.CompressedSize, actual.CompressedSize);
            Assert.Equal(peeked.UncompressedSize, actual.UncompressedSize);
            Assert.Equal(peeked.FileNameLength, actual.FileNameLength);
            Assert.Equal(peeked.ExtraFieldLength, actual.ExtraFieldLength);
            Assert.Equal(peeked.FileName, actual.FileName);
        }

        /// <summary>
        /// Test demonstrating that PeekString (non-generic method) works correctly.
        /// This shows the pattern that Peek methods SHOULD follow: non-destructive reads.
        /// </summary>
        [Fact]
        public void PeekString_DoesNotAdvancePointer()
        {
            byte[] data = {
                0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00
            };

            var slide = new ByteSlide(data);

            // Peek at the string
            string peeked = slide.PeekString(Encoding.ASCII, 5);
            Assert.Equal("Hello", peeked);

            // Read the string (should get same data)
            string read = slide.GetString(Encoding.ASCII, 5);
            Assert.Equal("Hello", read);

            // If pointer wasn't advanced by peek, the next 6 bytes should be " World"
            string next = slide.GetString(Encoding.ASCII, 6);
            Assert.Equal(" World", next);
        }



       
    }
}
