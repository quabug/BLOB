using System;
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
#if UNITY_2021_2_OR_NEWER
            stream.Write(new ReadOnlySpan<byte>(valuePtr, size));
#else
            for (var i = 0; i < size; i++) stream.WriteByte(*(valuePtr + i));
#endif
        }
    }
}