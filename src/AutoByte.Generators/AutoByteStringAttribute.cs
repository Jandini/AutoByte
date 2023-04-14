using System.Text;

namespace AutoByte
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoByteStringAttribute : AutoByteFieldAttribute 
    {
        public Encoding Encoding { get; set; }
    }
}
