using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class BlobNullTerminatedStringBuilder<TEncoding> : BlobArrayBuilder<byte, BlobNullTerminatedString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public BlobNullTerminatedStringBuilder() : base(new byte[] { 0 }) {}
        // TODO: optimize?
        public BlobNullTerminatedStringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str).Append((byte)0)) {}
    }
}