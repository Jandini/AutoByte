using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace AutoByte
{
    public ref struct ByteSlide
    {

        private ReadOnlySpan<byte> _slide;

        public ByteSlide(byte[] buffer)
        {
            _slide = new Span<byte>(buffer);
        }

        public ByteSlide(ReadOnlySpan<byte> span)
        {
            _slide = span;
        }

        public readonly int Length { get => _slide.Length; }

        /// <summary>
        /// Move pointer by "sliding" forward by specified number of bytes.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="ByteSlideException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> Slide(int size)
        {
            if (size > _slide.Length)
                throw new ByteSlideException($"Cannot slide {size} bytes. There is {_slide.Length} bytes left.");

            var slice = _slide[..size];
            _slide = _slide[size..];
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Skip(int size) {

            if (size > _slide.Length)
                throw new ByteSlideException($"Cannot skip {size} bytes. There is {_slide.Length} bytes left.");

            _slide = _slide[size..];            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte() => Slide(sizeof(byte))[0];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetByte<T>() => (T)(object)Slide(sizeof(byte))[0];
       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetInt16LittleEndian() => BinaryPrimitives.ReadInt16LittleEndian(Slide(sizeof(short)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt32LittleEndian() => BinaryPrimitives.ReadInt32LittleEndian(Slide(sizeof(int)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetInt64LittleEndian() => BinaryPrimitives.ReadInt64LittleEndian(Slide(sizeof(long)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt16LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadInt16LittleEndian(Slide(sizeof(short)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt32LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadInt32LittleEndian(Slide(sizeof(int)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt64LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadInt64LittleEndian(Slide(sizeof(long)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUInt16LittleEndian() => BinaryPrimitives.ReadUInt16LittleEndian(Slide(sizeof(ushort)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt32LittleEndian() => BinaryPrimitives.ReadUInt32LittleEndian(Slide(sizeof(uint)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetUInt64LittleEndian() => BinaryPrimitives.ReadUInt64LittleEndian(Slide(sizeof(ulong)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt16LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(Slide(sizeof(ushort)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt32LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(Slide(sizeof(uint)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt64LittleEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt64LittleEndian(Slide(sizeof(ulong)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetInt16BigEndian() => BinaryPrimitives.ReadInt16BigEndian(Slide(sizeof(short)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt32BigEndian() => BinaryPrimitives.ReadInt32BigEndian(Slide(sizeof(int)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetInt64BigEndian() => BinaryPrimitives.ReadInt64BigEndian(Slide(sizeof(long)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt16BigEndian<T>() => (T)(object)BinaryPrimitives.ReadInt16BigEndian(Slide(sizeof(short)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt32BigEndian<T>() => (T)(object)BinaryPrimitives.ReadInt32BigEndian(Slide(sizeof(int)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetInt64BigEndian<T>() => (T)(object)BinaryPrimitives.ReadInt64BigEndian(Slide(sizeof(long)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUInt16BigEndian() => BinaryPrimitives.ReadUInt16BigEndian(Slide(sizeof(ushort)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt32BigEndian() => BinaryPrimitives.ReadUInt32BigEndian(Slide(sizeof(uint)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetUInt64BigEndian() => BinaryPrimitives.ReadUInt64BigEndian(Slide(sizeof(ulong)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt16BigEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt16BigEndian(Slide(sizeof(ushort)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt32BigEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt32BigEndian(Slide(sizeof(uint)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUInt64BigEndian<T>() => (T)(object)BinaryPrimitives.ReadUInt64BigEndian(Slide(sizeof(ulong)));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(Encoding encoding, int length) => encoding.GetString(Slide(length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetUtf8String(int size) => Encoding.UTF8.GetString(Slide(size));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetUnicodeString(int size) => Encoding.Unicode.GetString(Slide(size));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetByteArray(int length) => Slide(length).ToArray();




        /// <summary>
        /// Represents the UTC epoch date for CDate values.
        /// This epoch is used as a reference point for interpreting CDate values.
        /// </summary>
        private static readonly DateTime _cDateUtcEpoch = new(621355968000000000, DateTimeKind.Utc);

        /// <summary>
        /// Represents the UTC epoch date for HFSDate values.
        /// This epoch is used as a reference point for interpreting HFSDate values.
        /// </summary>
        private static readonly DateTime _hfsDateUtcEpoch = new(600527520000000000, DateTimeKind.Utc);


        /// <summary>
        /// Retrieves a DateTime value in UTC by interpreting the next 4 bytes in little-endian format as seconds since the CDate UTC epoch.
        /// </summary>
        /// <returns>A DateTime value in UTC.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetCDateUtcLittleEndian() => _cDateUtcEpoch.AddSeconds(BinaryPrimitives.ReadUInt32LittleEndian(Slide(sizeof(uint))));

        /// <summary>
        /// Retrieves a DateTime value in UTC by interpreting the next 4 bytes in big-endian format as seconds since the CDate UTC epoch.
        /// </summary>
        /// <returns>A DateTime value in UTC.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetCDateUtcBigEndian() => _cDateUtcEpoch.AddSeconds(BinaryPrimitives.ReadUInt32BigEndian(Slide(sizeof(uint))));

        /// <summary>
        /// Retrieves a DateTime value in UTC by interpreting the next 4 bytes in little-endian format as seconds since the HFSDate UTC epoch.
        /// </summary>
        /// <returns>A DateTime value in UTC.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetHfsDateUtcLittleEndian() => _hfsDateUtcEpoch.AddSeconds(BinaryPrimitives.ReadUInt32LittleEndian(Slide(sizeof(uint))));

        /// <summary>
        /// Retrieves a DateTime value in UTC by interpreting the next 4 bytes in big-endian format as seconds since the HFSDate UTC epoch.
        /// </summary>
        /// <returns>A DateTime value in UTC.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetHfsDateUtcBigEndian() => _hfsDateUtcEpoch.AddSeconds(BinaryPrimitives.ReadUInt32BigEndian(Slide(sizeof(uint))));



        /// <summary>
        /// Retrieves a null-terminated C string from the ByteSlide, decoding it using the specified encoding.
        /// </summary>
        /// <param name="encoding">The character encoding to use for decoding the C string.</param>
        /// <returns>The decoded C string.</returns>
        /// <exception cref="ByteSlideException">Thrown if the C string is not properly terminated with a null character.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCString(Encoding encoding)
        {
            // Find the index of the first zero byte (null terminator) in the _slide Span<byte>.
            var zeroIndex = _slide.IndexOf((byte)0);

            if (zeroIndex != -1)
            {
                // Extract the portion of _slide representing the C string.
                var stringBytes = Slide(zeroIndex);

                // Skip the null terminator.
                Skip(1);

                // Decode the C string to a string using the specified encoding and return it.
                return encoding.GetString(stringBytes);
            }

            // If no null terminator was found, throw an exception indicating an unterminated C string.
            throw new ByteSlideException("Unterminated C string.");
        }



        /// <summary>
        /// Retrieves a null-terminated C string from the ByteSlide, decoding it using the specified encoding.
        /// </summary>
        /// <param name="encoding">The character encoding to use for decoding the C string.</param>
        /// <param name="maxLength">The maximum length of the C string to retrieve.</param>
        /// <returns>The decoded C string, which may be truncated if it exceeds the specified maximum length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCString(Encoding encoding, int maxLength)
        {
            // Find the index of the first zero byte (null terminator) in the _slide Span<byte>.
            var zeroIndex = _slide.IndexOf((byte)0);

            if (zeroIndex != -1 && (zeroIndex - 1) <= maxLength)
            {
                // Extract the portion of _slide representing the C string.

                var stringBytes = Slide(zeroIndex);

                // Skip the null terminator.

                Skip(1);

                // Decode the C string to a string using the specified encoding and return it.
                return encoding.GetString(stringBytes);
            }

            // If no null terminator was found or if the string exceeds maxLength, retrieve a string of maximum length.

            return encoding.GetString(Slide(maxLength));
        }



        /// <summary>
        /// Retrieves a byte array from the current pointer position and aligns the pointer to the specified value.
        /// </summary>
        /// <param name="length">The length of the byte array to retrieve.</param>
        /// <param name="align">The number of bytes to align the pointer to after retrieving the array.</param>
        /// <returns>An array of bytes with the specified length.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetByteArrayAlignTo(int length, int align)
        {
            // Retrieve a byte array of the specified length from the current pointer position.
            var result = Slide(length).ToArray();

            // Calculate the alignment difference.
            var alignment = length % align;

            // If there is an alignment difference, move the pointer forward by the difference.
            if (alignment > 0)
                _slide = _slide[alignment..];

            return result;
        }


        /// <summary>
        /// Deserializes and retrieves a structure of type T from the current slide, ensuring proper alignment and handling of structure size.
        /// </summary>
        /// <typeparam name="T">The type of the structure to retrieve, which must implement the IByteStructure interface and have a default constructor.</typeparam>
        /// <returns>An instance of the specified structure type, deserialized from the current slide.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetStructure<T>() where T : IByteStructure, new()
        {
            int more;

            // Create a new object to represent the structure.
            var structure = new T();

            // Get the current slide location.
            var start = _slide.Length;

            // Map the structure to the object and get the expected size.
            var size = structure.Deserialize(ref this);

            // If the expected structure size was provided, then check if more slide is required.
            if (size > 0)
            {
                more = (start - _slide.Length) % size;

                // If there are remaining bytes, ensure they are sufficient for the structure size.
                if (more > 0)
                {
                    if (more > _slide.Length)
                        throw new ByteSlideException($"The structure size ({size} bytes) is too big. There is {_slide.Length} bytes left.");

                    // Adjust the slide to skip past any remaining bytes.
                    _slide = _slide[more..];
                }
            }

            return structure;
        }

        /// <summary>
        /// Align the pointer to the specified number of bytes based on the size of the previous structure
        /// </summary>
        /// <param name="size">Size of the previous structure</param>
        /// <param name="alignment">Align to specified number of bytes</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlignTo(int size, int align)
        {
            var alignment = size % align;

            if (alignment > 0)
                _slide = _slide[alignment..];
        }



        /// <summary>
        /// Peek string without moving the byte pointer.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string PeekString(Encoding encoding, int length) => encoding.GetString(_slide[..length]);

        /// <summary>
        /// Get structure from current position without sliding. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PeekStructure<T>() where T : IByteStructure, new()
        {
            var structure = new T();
            structure.Deserialize(ref this);
            return structure;
        }
    }
}