namespace Blob
{
    public unsafe struct BlobPtr<T> where T : unmanaged
    {
        internal int Offset;

        public ref T Value
        {
            get
            {
                fixed (void* ptr = &Offset)
                {
                    return ref *(T*)((byte*)ptr + Offset);
                }
            }
        }

        public T* UnsafePtr
        {
            get
            {
                fixed (void* thisPtr = &Offset)
                {
                    return (T*)((byte*) thisPtr + Offset);
                }
            }
        }
    }
}