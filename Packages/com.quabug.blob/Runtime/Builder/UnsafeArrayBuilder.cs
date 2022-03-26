using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class UnsafeArrayBuilder<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        static UnsafeArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        private byte* _ptr;
        private int _length;

        public UnsafeArrayBuilder(TValue* arrayPtr, int length)
        {
            _ptr = (byte*)arrayPtr;
            _length = sizeof(TValue) * length;
        }

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            stream.WriteValue(ref _length);

            var arrayPatchPosition = Utilities.Align<TValue>(patchPosition + sizeof(TValue) * _length);
            stream.SetLength(arrayPatchPosition + 1);

            stream.Seek(patchPosition, SeekOrigin.Begin);
            for (var i = 0; i < _length; i++) stream.WriteByte(*(_ptr + i));
            return arrayPatchPosition;
        }
    }
}