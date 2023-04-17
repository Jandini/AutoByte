namespace AutoByte
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoByteStringAttribute : AutoByteFieldAttribute 
    {
        /// <summary>
        /// UTF8, ASCII, Unicode, BigEndianUnicode, UTF32, UTF7 
        /// 
        /// Encoding class as of.NET 5
        /// UTF8 without BOM,
        /// UTF32BE,
        /// UTF32LE,
        /// </summary>
        public string Encoding { get; set; }
    }
}
