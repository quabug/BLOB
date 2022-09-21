using System;
using AnySerialize;
using UnityEngine;

namespace Blob
{
    [Serializable]
    public class AnyManagedBlobReference<T, [AnyConstraintType] TAnyBlob> : IReadOnlyAny<ManagedBlobAssetReference<T>>
        where T : unmanaged
        where TAnyBlob : IReadOnlyAnyBlob<T>
    {
        [SerializeField] private TAnyBlob _blob = default!;
        private ManagedBlobAssetReference<T> _cache;
        public ManagedBlobAssetReference<T> Value
        {
            get
            {
#if !UNITY_EDITOR
                if (_cache != null) return _cache;
#endif
                using var stream = new BlobMemoryStream();
                _blob.Build(stream);
                _cache = new ManagedBlobAssetReference<T>(stream.ToArray());
                return _cache;
            }
        }
    }
}