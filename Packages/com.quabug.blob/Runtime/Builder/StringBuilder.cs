using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class StringBuilder<TEncoding> : ArrayBuilder<byte, BlobString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public StringBuilder() {}
        // TODO: optimize?
        public StringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str)) {}
    }
}