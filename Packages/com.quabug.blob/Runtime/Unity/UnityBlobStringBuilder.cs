#if UNITY_BLOB

using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobStringBuilder : ArrayBuilder<byte, Unity.Entities.BlobString>
    {
        public UnityBlobStringBuilder() {}
        public UnityBlobStringBuilder([NotNull] string str) : base(Encoding.UTF8.GetBytes(str).Append((byte)0), 4) {}
    }
}

#endif