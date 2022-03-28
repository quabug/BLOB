#if UNITY_BLOB

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobArrayBuilderWithItemBuilders<T> : ArrayBuilderWithItemBuilders<T, Unity.Entities.BlobArray<T>>
        where T : unmanaged
    {
        public UnityBlobArrayBuilderWithItemBuilders([NotNull] IEnumerable<IBuilder<T>> builders) : base(builders) {}
    }
}

#endif