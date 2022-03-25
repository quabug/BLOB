#if UNITY_BLOB

using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class EntitiesBlobStringBuilder : BlobArrayBuilder<byte, Unity.Entities.BlobString>
    {
        public EntitiesBlobStringBuilder() {}
        public EntitiesBlobStringBuilder([NotNull] string str) : base(Encoding.UTF8.GetBytes(str).Append((byte)0)) {}
    }
}

#endif