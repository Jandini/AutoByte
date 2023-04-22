namespace AutoByte
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class AutoByteFieldAttribute : Attribute
    {
        /// <summary>
        /// Skip number of bytes before reading the field.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Size of the field or length of the string or array.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Use deserialized property value as the size of decorated field. 
        /// </summary>
        public string SizeFromProperty { get; set; }
    }
}
