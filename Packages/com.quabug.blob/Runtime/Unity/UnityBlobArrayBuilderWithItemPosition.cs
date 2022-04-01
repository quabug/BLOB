#if UNITY_BLOB

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobArrayBuilderWithItemPosition<T> : ArrayBuilderWithItemPosition<T, Unity.Entities.BlobArray<T>> where T : unmanaged
    {
        public UnityBlobArrayBuilderWithItemPosition() {}
        public UnityBlobArrayBuilderWithItemPosition([NotNull] IEnumerable<T> elements) : base(elements) {}
        public UnityBlobArrayBuilderWithItemPosition([NotNull] T[] elements) : base(elements) {}
    }
}

#endif