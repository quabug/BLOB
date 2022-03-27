using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public static class StreamExtension
    {
        public static unsafe void WriteValue<T>([NotNull] this Stream stream, ref T value) where T : unmanaged
        {
            fixed (T* valuePtr = &value)
            {
                var size = sizeof(T);
                WriteValuePtr(stream, (byte*)valuePtr, size);
            }
        }

        public static unsafe void WriteValuePtr([NotNull] this Stream stream, byte* valuePtr, int size)
        {
            // TODO: should handle endianness?
            using var unmanagedMemoryStream = new UnmanagedMemoryStream(valuePtr, size);
            unmanagedMemoryStream.CopyTo(stream);
        }
    }
}