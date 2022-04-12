using System;

namespace Blob
{
    public unsafe struct BlobPtrAny
    {
        internal BlobArray<byte> Data;

        public ref T GetValue<T>() where T : unmanaged
        {
            return ref *GetUnsafeValuePtr<T>();
        }

        public T* GetUnsafeValuePtr<T>() where T : unmanaged
        {
            if (sizeof(T) != Data.Length) throw new ArgumentException("invalid generic parameter");
            return (T*)Data.UnsafePtr;
        }

        public int Size => Data.Length;
        public void* UnsafePtr => Data.UnsafePtr;
    }
}