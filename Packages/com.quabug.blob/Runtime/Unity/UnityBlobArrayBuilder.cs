#if UNITY_BLOB

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobArrayBuilder<T> : ArrayBuilder<T, Unity.Entities.BlobArray<T>> where T : unmanaged
    {
        public bool UseUnityBlobDataAlignment
        {
            set
            {
                if (value) DataAlignment = Utilities.AlignOf<T>();
            }
        }
        
        public UnityBlobArrayBuilder() {}
        public UnityBlobArrayBuilder([NotNull] IEnumerable<T> elements) : base(elements) {}
        public UnityBlobArrayBuilder([NotNull] T[] elements) : base(elements) {}
    }
}

#endif