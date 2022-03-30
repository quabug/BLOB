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
                return ref *(UnsafePtr + index);
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

        public T[] ToArray()
        {
            var array = new T[Length];
            // TODO: benchmark
            for (var i = 0; i < Length; i++) array[i] = this[i];
            return array;
        }

#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
        public Span<T> ToSpan()
        {
            return new Span<T>(UnsafePtr, Length);
        }
#endif
    }
}