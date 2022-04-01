using System;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class StringBuilder<TEncoding> : ArrayBuilder<byte, BlobString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public StringBuilder() : base(Array.Empty<byte>(), 4) {}
        public StringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str), 4) {}
    }
}