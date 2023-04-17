using AutoByte;
using System.Drawing;
using System.Text;

namespace Demo
{
    public enum DemoType : byte
    {
        First = 0, 
        Last = 1,
    }

    [AutoByteStructure(Size = 45, IsBigEndian = true)]
    internal partial class DemoStructure
    {
        [AutoByteField(Size = 10)]
        public byte[] Data { get; set; }
        public DemoType Length { get; set; }
        public int Size { get; set; }

        [AutoByteField(Skip = 3)]
        public ulong Length2 { get; set; }

        [AutoByteString(CodePage = 1250, Size = 11)]
        public string Text { get; set; }
    }
}
