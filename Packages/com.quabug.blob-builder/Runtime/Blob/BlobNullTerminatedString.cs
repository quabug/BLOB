using System.Text;

namespace Blob
{
    public unsafe struct BlobNullTerminatedString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public new string ToString() => new TEncoding().GetString(Data.GetUnsafePtr(), Data.Length-1);
    }
}