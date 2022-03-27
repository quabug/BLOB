using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class UnsafeArrayBuilder<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly TValue[] _array;

        static UnsafeArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public UnsafeArrayBuilder([NotNull] TValue[] array)
        {
            _array = array;
        }

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            var length = _array.Length;
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            stream.WriteValue(ref length);
            if (length == 0) return patchPosition;

            stream.Seek(patchPosition, SeekOrigin.Begin);
            var size = sizeof(TValue) * length;
            fixed (void* arrayPtr = &_array[0])
            {
                using var unmanagedMemoryAccessor = new UnmanagedMemoryStream((byte*)arrayPtr, size);
                unmanagedMemoryAccessor.CopyTo(stream);
            }
            return Utilities.Align<TValue>(patchPosition + size);
        }
    }
}