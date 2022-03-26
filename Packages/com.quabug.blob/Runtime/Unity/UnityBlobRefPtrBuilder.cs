#if UNITY_BLOB

namespace Blob
{
    public class UnityBlobRefPtrBuilder<T> : RefPtrBuilder<T, Unity.Entities.BlobPtr<T>> where T : unmanaged
    {
        public UnityBlobRefPtrBuilder(IBuilder<T> builder) : base(builder) {}
    }
}

#endif