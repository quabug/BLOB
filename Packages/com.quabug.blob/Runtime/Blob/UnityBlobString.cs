using System.Text;

namespace Blob
{
    public unsafe struct UnityBlobString
    {
        internal BlobNullTerminatedString<UTF8Encoding> Data;
        public int Length => Data.Length;
        public byte* UnsafePtr => Data.UnsafePtr;
        public new string ToString() => Data.ToString();
    }
}