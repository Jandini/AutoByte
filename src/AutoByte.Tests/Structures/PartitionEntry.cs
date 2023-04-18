namespace AutoByte.Tests.Structures
{
    [AutoByteStructure]
    internal partial class PartitionEntry
    {
        public byte BootIndicator { get; set; }
        [AutoByteField(Size = 3)]
        public byte[] StartingCHS { get; set; }     
        public PartitionType Type { get; set; }

        [AutoByteField(Size = 3)]
        public byte[] EndingCHS { get; set; }
        public uint StartingLBA { get; set; }
        public uint SizeInLBA { get; set; }        
    }
}
