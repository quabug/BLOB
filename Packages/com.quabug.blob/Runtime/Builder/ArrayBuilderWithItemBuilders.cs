using System;
using System.Collections.Generic;
using System.IO;
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

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            var length = _builders.Length;
            stream.WriteValue(ref offset);
            stream.WriteValue(ref length);
            var arrayPatchPosition = Utilities.Align<TValue>(patchPosition + sizeof(TValue) * length);
            for (var i = 0; i < length; i++)
            {
                var arrayDataPosition = patchPosition + sizeof(TValue) * i;
                arrayPatchPosition = _builders[i].Build(stream, arrayDataPosition, arrayPatchPosition);
            }

            return arrayPatchPosition;
        }
    }

    public class ArrayBuilderWithItemBuilders<T> : ArrayBuilderWithItemBuilders<T, BlobArray<T>> where T : unmanaged
    {
        public ArrayBuilderWithItemBuilders([NotNull, ItemNotNull] IEnumerable<IBuilder<T>> builders) : base(builders) {}
    }
}