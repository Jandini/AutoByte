using System.Runtime.InteropServices;

namespace AutoByte.Tests.Structures
{
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZipFileHeaderStructure
    {
        /// <summary>
        /// Signature for ZIP file header
        /// </summary>
        public int Signature;

        /// <summary>
        /// Version of software needed to extract ZIP file
        /// </summary>
        public short VersionNeededToExtract;

        /// <summary>
        /// General purpose bit flag
        /// </summary>
        public short GeneralPurposeBitFlag;

        /// <summary>
        /// Compression method used
        /// </summary>
        public short CompressionMethod;

        /// <summary>
        /// Last modified file time
        /// </summary>
        public short LastModifiedFileTime;

        /// <summary>
        /// Last modified file date
        /// </summary>
        public short LastModifiedFileDate;

        /// <summary>
        /// CRC-32 checksum of file
        /// </summary>
        public int Crc32;

        /// <summary>
        /// Compressed size of file
        /// </summary>
        public int CompressedSize;

        /// <summary>
        /// Uncompressed size of file
        /// </summary>
        public int UncompressedSize;

        /// <summary>
        /// Length of file name
        /// </summary>
        public short FileNameLength;

        /// <summary>
        /// Length of extra field
        /// </summary>
        public short ExtraFieldLength;

        /// <summary>
        /// File name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] FileName;
    }
}
