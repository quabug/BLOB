#if UNITY_BLOB

using System.Collections.Generic;

namespace Blob
{
    public class UnityBlobArrayBuilder<T> : ArrayBuilder<T, Unity.Entities.BlobArray<T>> where T : unmanaged
    {
        public UnityBlobArrayBuilder() {}
        public UnityBlobArrayBuilder(IEnumerable<T> elements) : base(elements) {}
        public UnityBlobArrayBuilder(IEnumerable<IBuilder<T>> builders) : base(builders) {}
    }
}

#endif