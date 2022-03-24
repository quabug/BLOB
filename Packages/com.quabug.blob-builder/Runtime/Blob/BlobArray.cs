using System;

namespace Blob
{
    public unsafe struct BlobArray<T> where T : unmanaged
    {
        internal int Offset;
        private int _length;
        public int Length { get => _length; internal set => _length = value; }

        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException($"index({index}) out of range[0-{Length})");
                fixed (void* thisPtr = &Offset)
                {
                    return ref *(T*)((byte*)thisPtr + index * sizeof(T));
                }
            }
        }

        public T* GetUnsafePtr()
        {
            fixed (void* thisPtr = &Offset)
            {
                return (T*)((byte*) thisPtr + Offset);
            }
        }
    }
}