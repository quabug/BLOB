#if UNITY_BLOB

namespace Blob
{
    public class UnityBlobPtrBuilderWithNewValue<T> : PtrBuilderWithNewValue<T, Unity.Entities.BlobPtr<T>> where T : unmanaged
    {
        public UnityBlobPtrBuilderWithNewValue() {}
        public UnityBlobPtrBuilderWithNewValue(T value) : base(value) {}
        public UnityBlobPtrBuilderWithNewValue(IBuilder<T> builder) : base(builder) {}
    }
}

#endif