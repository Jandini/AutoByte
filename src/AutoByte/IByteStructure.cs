namespace AutoByte
{
    public partial interface IByteStructure
    {
        /// <summary>
        /// Deserialize bytes using byte slide.
        /// </summary>
        /// <param name="slide">Byte slide deserializer</param>
        /// <returns>Return the size of the structure to move the byte pointer by the size of the structure. Return zero to skip moving the byte pointer.</returns>
        int Deserialize(ref ByteSlide slide);
    }
}