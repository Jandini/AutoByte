namespace AutoByte.Tests.Structures
{
    public class LicenseText : IByteStructure
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
}
