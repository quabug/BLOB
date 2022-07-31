using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class ArrayBuilderWithItemBuilders<TValue, TArray> : Builder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly IBuilder<TValue>[] _builders;

        static ArrayBuilderWithItemBuilders()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public ArrayBuilderWithItemBuilders([NotNull, ItemNotNull] IEnumerable<IBuilder<TValue>> builders) => _builders = builders.ToArray();

        public IBuilder<TValue> this[int index] => _builders[index];

        protected override void BuildImpl(IBlobStream stream, ref TArray data)
        {
            stream.WriteArray(_builders);
        }
    }

    public class ArrayBuilderWithItemBuilders<T> : ArrayBuilderWithItemBuilders<T, BlobArray<T>> where T : unmanaged
    {
        public ArrayBuilderWithItemBuilders([NotNull, ItemNotNull] IEnumerable<IBuilder<T>> builders) : base(builders) {}
    }
}