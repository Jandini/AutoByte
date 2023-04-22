using AutoByte.Tests.Structures;
using System.Runtime.InteropServices;

namespace AutoByte.Tests
{

    public class AutoByte_Must
    {

        [Fact]
        public void DeserializeWith_ByteSlide()
        {
            var data = new byte[]
            {
                0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54,
                0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08,
                0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79,
                0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
            };

            for (int i = 0; i < 100000; i++)
            {
                var zip = new ByteSlide(data).GetStructure<ZipFileHeader>();
            }

        }

        [Fact]
        public void DeserializeWith_PtrToStructure()
        {
            var data = new byte[]
        {
                0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00, 0x63, 0x54,
                0x96, 0x56, 0x45, 0x7F, 0x6A, 0xBD, 0x5B, 0x02, 0x00, 0x00, 0xF4, 0x08,
                0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x41, 0x75, 0x74, 0x6F, 0x42, 0x79,
                0x74, 0x65, 0x2E, 0x73, 0x6C, 0x6E
        };

            for (int i = 0; i < 100000; i++)
            {
                var zip = ToStructure<ZipFileHeaderStructure>(data);
            }

        }


        private static T ToStructure<T>(byte[] data)
        {
            IntPtr buffer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buffer, data.Length);
            T result = Marshal.PtrToStructure<T>(buffer);
            Marshal.FreeHGlobal(buffer);

            return result;
        }
    }
}