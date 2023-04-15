using AutoByte;

namespace Demo
{

    public enum DemoType : byte
    {
        First = 0, 
        Last = 1,
    }

    [AutoByteStructure(Size = 45, IsBigEndian = true)]
    public partial class DemoStructure
    {        
        public DemoType Length { get; set; }
        public int Length1 { get; set; }
        
        [AutoByteString(Skip = 3)]
        public ulong Length2 { get; set; }

        //[AutoByteString(Size = 10)]
        //public string Text { get; set; }
    }
}
