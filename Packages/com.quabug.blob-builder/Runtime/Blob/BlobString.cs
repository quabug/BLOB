using System.Text;

namespace Blob
{
    public unsafe struct BlobString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public new string ToString() => new TEncoding().GetString(Data.GetUnsafePtr(), Data.Length);
    }
}