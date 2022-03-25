using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class BlobStringBuilder<TEncoding> : BlobArrayBuilder<byte, BlobString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public BlobStringBuilder() {}
        // TODO: optimize?
        public BlobStringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str)) {}
    }
}