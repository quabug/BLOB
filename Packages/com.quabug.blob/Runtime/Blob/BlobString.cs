using System.Text;

namespace Blob
{
    public unsafe struct BlobString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public int Length => Data.Length;
        public byte* UnsafePtr => Data.UnsafePtr;
        public new string ToString() => new TEncoding().GetString(Data.UnsafePtr, Data.Length);
    }
}