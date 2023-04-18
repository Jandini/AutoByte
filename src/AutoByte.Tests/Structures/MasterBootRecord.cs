namespace AutoByte.Tests.Structures
{


    internal class MasterBootRecord : IByteStructure
    {
        public byte[] BootCode { get; private set; }
        public ushort BootSignature { get; private set; }
        public PartitionEntry[] PartitionEntries { get; private set; } 

        public int Deserialize(ref ByteSlide slide)
        {
            BootCode = slide.GetByteArray(446);
            
            PartitionEntries = new PartitionEntry[4];           
            for (int i = 0; i < 4; i++)
                PartitionEntries[i] = slide.GetStructure<PartitionEntry>();

            BootSignature = slide.GetUInt16LittleEndian();

            return 0;
        }
    }
}
