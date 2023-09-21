namespace AutoByte
{
    public static class ByteExtensions
    {
        /// <summary>
        /// Create instance of T:IByteStrcture and execute Map method with ByteSlide.
        /// </summary>
        /// <typeparam name="T">Output class type</typeparam>
        /// <param name="span">Input bytes as span</param>
        /// <returns>Deserialized bytes as given T object.</returns>
        public static T GetStructure<T>(this ReadOnlySpan<byte> span) where T : IByteStructure, new()
        {
            var structure = new T();
            var slide = new ByteSlide(span);
            structure.Deserialize(ref slide);
            return structure;
        }
    }
}
