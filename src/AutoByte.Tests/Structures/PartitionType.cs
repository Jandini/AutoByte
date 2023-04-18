namespace AutoByte.Tests.Structures
{
    public enum PartitionType : byte
    {
        Empty = 0x00,
        Fat12 = 0x01,
        XENIXRoot = 0x02,
        XENIXUser = 0x03,
        FAT16 = 0x04,
        Extended = 0x05,
        FAT32 = 0x0B,
        FAT32LBA = 0x0C,
        Linux = 0x83,
    }
}
