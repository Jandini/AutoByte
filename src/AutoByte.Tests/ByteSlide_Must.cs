using System.Buffers.Binary;

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

    }
}
