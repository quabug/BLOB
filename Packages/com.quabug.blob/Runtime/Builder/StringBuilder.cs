using System;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class StringBuilder<TEncoding> : UnsafeArrayBuilder<byte, BlobString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public StringBuilder() : base(Array.Empty<byte>()) {}
        public StringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str)) {}
    }
}