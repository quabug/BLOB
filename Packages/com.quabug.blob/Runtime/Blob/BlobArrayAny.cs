using System;

namespace Blob
{
    public unsafe struct BlobArrayAny
    {
        public BlobArray<int> Offsets; // Offsets.Last == Data.Length
        public BlobArray<byte> Data;

        public int Length => Offsets.Length - 1;

        public int GetOffset(int index)
        {
            return Offsets[index];
        }

        public ref T GetValue<T>(int index) where T : unmanaged
        {
            return ref *GetUnsafeValuePtr<T>(index);
        }

        public T* GetUnsafeValuePtr<T>(int index) where T : unmanaged
        {
            if (GetSize(index) < sizeof(T)) throw new ArgumentException("invalid generic parameter");
            return (T*)GetUnsafeValuePtr(index);
        }

        public void* GetUnsafeValuePtr(int index)
        {
            return Data.UnsafePtr + Offsets[index];
        }

        public int GetSize(int index)
        {
            return Offsets[index + 1] - Offsets[index];
        }
    }
}