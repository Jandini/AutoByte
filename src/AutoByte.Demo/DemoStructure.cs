using AutoByte;

namespace Demo
{
    [AutoByteStructure(Size = 45)]
    public partial class DemoStructure
    {        
        public Int32 Length { get; set; }
        public Int32 Length1 { get; set; }
        public Int32 Length2 { get; set; }

        //[AutoByteString(Size = 10)]
        //public string Text { get; set; }

    }
}
