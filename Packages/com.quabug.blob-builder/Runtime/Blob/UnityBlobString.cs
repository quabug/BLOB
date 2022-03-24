using System.Text;

namespace Blob
{
    public struct UnityBlobString
    {
        internal BlobNullTerminatedString<UTF8Encoding> Data;
        public new string ToString() => Data.ToString();
    }
}