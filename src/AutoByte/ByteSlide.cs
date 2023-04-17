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

        public int Length { get => _slide.Length; }

        /// <summary>
        /// Move pointer by "sliding" forward by specified number of bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ByteSlideException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> Slide(int length)
        {
            if (length > _slide.Length)
                throw new ByteSlideException("Byte slide is too short.");

            var slice = _slide[..length];
            _slide = _slide[length..];
            return slice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteSlide Skip(int length) { 
            _slide = _slide[length..]; 
            return this; 
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
        /// Get byte array from current pointer and align the pointer to given value.
        /// </summary>
        /// <param name="length">Array length</param>
        /// <param name="align">Align to given number of bytes</param>
        /// <returns>An array with given length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetByteArrayAlignTo(int length, int align)
        {
            var result = Slide(length).ToArray();
            
            var alignment = length % align;
            if (alignment > 0)
                _slide = _slide[alignment..];

            return result;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetStructure<T>() where T : IByteStructure, new()
        {
            int more;

            // create new object to represent the structure
            var structure = new T();

            // get current slide location 
            var start = _slide.Length;

            // map the structure to the object and get the expected size
            var size = structure.Deserialize(ref this);

            // if the expected structure size was provided then check if more slide is required 
            if (size > 0 && (more = (start - _slide.Length) % size) > 0)
                _slide = _slide[more..];

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