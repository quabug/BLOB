using System.Text;

namespace Blob
{
    public unsafe struct BlobNullTerminatedString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public int Length => Data.Length - 1/* length without termination */;
        public byte* UnsafePtr => Data.UnsafePtr;
        public new string ToString() => new TEncoding().GetString(Data.UnsafePtr, Length);
#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
        public System.Span<byte> ToSpan() => new System.Span<byte>(UnsafePtr, Length);
#endif
    }
}