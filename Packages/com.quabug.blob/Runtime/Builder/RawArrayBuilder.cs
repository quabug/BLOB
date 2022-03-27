using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class RawArrayBuilder<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly TValue[] _array;

        static RawArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public RawArrayBuilder([NotNull] TValue[] array)
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
#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
                stream.Write(new ReadOnlySpan<byte>(arrayPtr, size));
#else
                // `WriteByte` is the fastest way to write binary into stream on small chunks (<8192bytes) based on my benchmark
                // and BLOB intend to use on small chunks(? or maybe not?)
                // so I decide just use this simple way here
                for (var i = 0; i < size; i++) stream.WriteByte(*((byte*)arrayPtr + i));
#endif
            }
            return Utilities.Align<TValue>(patchPosition + size);
        }
    }

    public class RawArrayBuilder<TValue> : RawArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public RawArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}