namespace AutoByte
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoByteStringAttribute : AutoByteFieldAttribute 
    {
        /// <summary>
        /// UTF8, ASCII, Unicode, BigEndianUnicode, UTF32, UTF7
        /// 
        /// Add System.Text.Encoding.CodePages package. 
        /// Call Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        /// Use CodePage property to access other encodings.
        /// 
        /// ASCII(code page 20127), which is returned by the Encoding.ASCII property.
        /// ISO-8859-1 (code page 28591).
        /// UTF-7 (code page 65000), which is returned by the Encoding.UTF7 property.
        /// UTF-8 (code page 65001), which is returned by the Encoding.UTF8 property.
        /// UTF-16 and UTF-16LE (code page 1200), which is returned by the Encoding.Unicode property.
        /// UTF-16BE (code page 1201), which is instantiated by calling the UnicodeEncoding.UnicodeEncoding or UnicodeEncoding.UnicodeEncoding constructor with a bigEndian value of true.
        /// UTF-32 and UTF-32LE(code page 12000), which is returned by the Encoding.UTF32 property.
        /// UTF-32BE (code page 12001), which is instantiated by calling an UTF32Encoding constructor that has a bigEndian parameter and providing a value of true in the method call.
        /// </summary>
        public string Encoding { get; set; }
    }
}
