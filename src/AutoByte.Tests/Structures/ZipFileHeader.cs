namespace AutoByte.Tests.Structures
{
    [AutoByteStructure]
    public partial class ZipFileHeader
    {
        /// <summary>
        /// Signature for ZIP file header
        /// </summary>
        public int Signature { get; set; }

        /// <summary>
        /// Version of software used to create ZIP file
        /// </summary>
        public short VersionMadeBy { get; set; }

        /// <summary>
        /// Version of software needed to extract ZIP file
        /// </summary>
        public short VersionNeededToExtract { get; set; }

        /// <summary>
        /// General purpose bit flag
        /// </summary>
        public short GeneralPurposeBitFlag { get; set; }

        /// <summary>
        /// Compression method used
        /// </summary>
        public short CompressionMethod { get; set; }

        /// <summary>
        /// Last modified file time
        /// </summary>
        public short LastModifiedFileTime { get; set; }

        /// <summary>
        /// Last modified file date
        /// </summary>
        public short LastModifiedFileDate { get; set; }

        /// <summary>
        /// CRC-32 checksum of file
        /// </summary>
        public int Crc32 { get; set; }

        /// <summary>
        /// Compressed size of file
        /// </summary>
        public int CompressedSize { get; set; }

        /// <summary>
        /// Uncompressed size of file
        /// </summary>
        public int UncompressedSize { get; set; }

        /// <summary>
        /// Length of file name
        /// </summary>
        public short FileNameLength { get; set; }

        /// <summary>
        /// Length of extra field
        /// </summary>
        public short ExtraFieldLength { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        [AutoByteString(SizeFromProperty = "FileNameLength")]
        public string FileName { get; set; }

        /// <summary>
        /// Extra field
        /// </summary>
        [AutoByteField(SizeFromProperty = "ExtraFieldLength")]
        public byte[] ExtraField { get; set; }
    }
}
