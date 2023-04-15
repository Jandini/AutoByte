using AutoByte;

namespace Demo
{

    public enum DemoType : byte
    {
        First = 0, 
        Last = 1,
    }

    [AutoByteStructure(Size = 45)]
    public partial class DemoStructure
    {        
        public DemoType Length { get; set; }
        public Int32 Length1 { get; set; }
        public Int32 Length2 { get; set; }

        //[AutoByteString(Size = 10)]
        //public string Text { get; set; }

    }
}
