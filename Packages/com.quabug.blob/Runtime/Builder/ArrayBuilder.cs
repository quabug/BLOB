using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class ArrayBuilder<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly TValue[] _array;
        private readonly int _alignment;

        static ArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items, int alignment) : this(items.ToArray(), alignment) {}
        public ArrayBuilder([NotNull] TValue[] array) : this(array, Utilities.AlignOf<TValue>()) {}
        public ArrayBuilder([NotNull] TValue[] array, int alignment)
        {
            _array = array;
            _alignment = alignment;
            if (!Utilities.IsPowerOfTwo(_alignment)) throw new ArgumentException($"{nameof(alignment)} must be power of 2");
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            stream.EnsureDataSize<TArray>()
                .WritePatchOffset()
                .WriteValue(_array.Length)
                .ToPatchPosition()
                .WriteArray(_array)
                .AlignPatch(_alignment)
            ;
        }
    }

    public class ArrayBuilder<TValue> : ArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}