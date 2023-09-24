using System.Buffers.Binary;
using System.Text;

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



       
    }
}
