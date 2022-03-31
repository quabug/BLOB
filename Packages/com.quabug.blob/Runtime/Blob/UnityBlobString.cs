using System.Text;

namespace Blob
{
    public unsafe struct UnityBlobString
    {
        internal BlobNullTerminatedString<UTF8Encoding> Data;
        public int Length => Data.Length;
        public byte* UnsafePtr => Data.UnsafePtr;
        public new string ToString() => Data.ToString();
#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
        public System.Span<byte> ToSpan() => Data.ToSpan();
#endif
    }
}