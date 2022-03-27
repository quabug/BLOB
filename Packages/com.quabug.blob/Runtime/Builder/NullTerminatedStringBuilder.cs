using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class NullTerminatedStringBuilder<TEncoding> : UnsafeArrayBuilder<byte, BlobNullTerminatedString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public NullTerminatedStringBuilder() : base(new byte[] { 0 }) {}
        public NullTerminatedStringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str).Append((byte)0).ToArray()) {}
    }
}