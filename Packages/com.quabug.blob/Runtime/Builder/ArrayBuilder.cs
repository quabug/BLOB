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

        static ArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] TValue[] array) => _array = array;

        protected override void BuildImpl(IBlobStream stream, ref TArray data)
        {
            stream.WriteArray(_array, PatchAlignment);
        }
    }

    public class ArrayBuilder<TValue> : ArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}