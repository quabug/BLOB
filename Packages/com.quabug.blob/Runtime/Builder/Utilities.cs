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
            if (!PowerOfTwo(alignment))
                throw new ArgumentOutOfRangeException(nameof(alignment), "alignment must be power of 2");
            return (address + (alignment - 1)) & -alignment;

            static bool PowerOfTwo(int n)
            {
                return (n & (n - 1)) == 0;
            }
        }

        public static unsafe int AlignOf<T>() where T : unmanaged => sizeof(AlignHelper<T>) - sizeof(T);

        struct AlignHelper<T> where T : unmanaged
        {
            public byte _;
            public T Data;
        }

        public static unsafe long EnsurePatchPosition<T>(long patchPosition, long dataPosition) where T : unmanaged
        {
            return Align<T>(Math.Max(patchPosition, dataPosition + sizeof(T)));
        }
    }
}