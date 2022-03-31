using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class ArrayBuilder<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly TValue[] _array;
        private readonly ValuePositionBuilder[] _builders;

        static ArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public IBuilder<TValue> this[int index] => _builders[index];

        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] TValue[] array)
        {
            _array = array;
            _builders = new ValuePositionBuilder[array.Length];
            for (var i = 0; i < _builders.Length; i++) _builders[i] = new ValuePositionBuilder();
        }

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            var length = _array.Length;
            stream.WriteValue(offset);
            stream.WriteValue(length);
            if (length == 0) return patchPosition;

            stream.Seek(patchPosition, SeekOrigin.Begin);
            var valueSize = sizeof(TValue);
            var arraySize = valueSize * length;
            fixed (void* arrayPtr = &_array[0])
            {
#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
                stream.Write(new ReadOnlySpan<byte>(arrayPtr, arraySize));
#else
                // `WriteByte` is the fastest way to write binary into stream on small chunks (<8192bytes) based on my benchmark
                // and BLOB intend to use on small chunks(? or maybe not?)
                // so I decide just use this simple way here
                for (var i = 0; i < arraySize; i++) stream.WriteByte(*((byte*)arrayPtr + i));
#endif
            }

            for (var i = 0; i < length; i++) _builders[i].Position = patchPosition + valueSize * i;

            return Utilities.Align<TValue>(patchPosition + arraySize);
        }

        public class ValuePositionBuilder : IBuilder<TValue>
        {
            public long Build(Stream stream, long dataPosition, long patchPosition) => patchPosition;
            public long Position { get; internal set; }
        }
    }

    public class ArrayBuilder<TValue> : ArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}