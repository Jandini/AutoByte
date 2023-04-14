namespace AutoByte
{
    /// <summary>
    /// Use code generator to create IByteStructure deserialize implementation. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoByteStructureAttribute : Attribute
    {
        /// <summary>
        /// Structure size in bytes. This property is optional. 
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Apply BigEndian.
        /// </summary>
        public bool IsBigEndian { get; set; }
    }
}