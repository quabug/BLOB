#if UNITY_BLOB

namespace Blob
{
    public class UnityBlobPtrBuilderWithRefBuilder<T> : PtrBuilderWithRefBuilder<T, Unity.Entities.BlobPtr<T>> where T : unmanaged
    {
        public UnityBlobPtrBuilderWithRefBuilder(IBuilder<T> builder) : base(builder) {}
    }
}

#endif