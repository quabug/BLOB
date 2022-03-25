using System.Text;

namespace Blob
{
    public unsafe struct BlobNullTerminatedString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public int Length => Data.Length - 1/* length without termination */;
        public byte* UnsafePtr => Data.UnsafePtr;
        public new string ToString() => new TEncoding().GetString(Data.UnsafePtr, Length);
    }
}