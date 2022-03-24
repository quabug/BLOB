#if UNITY_BLOB

using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobStringBuilder : BlobArrayBuilder<byte, Unity.Entities.BlobString>
    {
        public UnityBlobStringBuilder() {}
        // TODO: optimize?
        public UnityBlobStringBuilder([NotNull] string str) : base(Encoding.UTF8.GetBytes(str).Append((byte)0)) {}
    }
}

#endif