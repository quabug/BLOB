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
        protected readonly TValue[] _Array;

        static ArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] TValue[] array) => _Array = array;

        protected override void BuildImpl(IBlobStream stream, ref TArray data)
        {
            stream.WriteArray(_Array, PatchAlignment);
        }
    }

    public class ArrayBuilder<TValue> : ArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilder() : this(Array.Empty<TValue>()) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}