using System;

namespace Blob
{
    public static class Utilities
    {
        public static long Align(long address)
        {
            return Align(address, IntPtr.Size);
        }

        public static long Align<T>(long address) where T : unmanaged
        {
            return Align(address, AlignOf<T>());
        }

        public static long Align(long address, int alignment)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), "alignment must be greater than 0");
            if (!IsPowerOfTwo(alignment))
                throw new ArgumentOutOfRangeException(nameof(alignment), "alignment must be power of 2");
            return (address + (alignment - 1)) & -alignment;
        }

        public static unsafe int AlignOf<T>() where T : unmanaged => sizeof(AlignHelper<T>) - sizeof(T);

        public static bool IsPowerOfTwo(int n) => (n & (n - 1)) == 0;

        struct AlignHelper<T> where T : unmanaged
        {
            public byte _;
            public T Data;
        }
    }
}