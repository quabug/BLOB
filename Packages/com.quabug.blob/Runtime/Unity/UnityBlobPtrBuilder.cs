#if UNITY_BLOB

namespace Blob
{
    public class UnityBlobPtrBuilder<T> : PtrBuilder<T, Unity.Entities.BlobPtr<T>> where T : unmanaged
    {
        public UnityBlobPtrBuilder() {}
        public UnityBlobPtrBuilder(T value) : base(value) {}
        public UnityBlobPtrBuilder(IBuilder<T> builder) : base(builder) {}
    }
}

#endif