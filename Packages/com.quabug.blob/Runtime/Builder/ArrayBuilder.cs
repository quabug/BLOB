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
        private readonly ValuePositionBuilder[] _builders;
        private readonly int _alignment;

        static ArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public IBuilder<TValue> this[int index] => _builders[index];

        public ArrayBuilder(int alignment = 0) : this(Array.Empty<TValue>(), alignment) {}
        public ArrayBuilder([NotNull] IEnumerable<TValue> items, int alignment = 0) : this(items.ToArray(), alignment) {}
        public ArrayBuilder([NotNull] TValue[] array, int alignment = 0)
        {
            _array = array;
            _builders = new ValuePositionBuilder[array.Length];
            for (var i = 0; i < _builders.Length; i++) _builders[i] = new ValuePositionBuilder();
            _alignment = alignment <= 0 ? Utilities.AlignOf<TValue>() : alignment;
            if (!Utilities.IsPowerOfTwo(_alignment)) throw new ArgumentException($"{nameof(alignment)} must be power of 2");
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            var patchPosition = stream.PatchPosition;
            var valueSize = sizeof(TValue);
            stream.EnsureDataSize<TArray>()
                .WritePatchOffset()
                .WriteValue(_array.Length)
                .ToPatchPosition()
                .WriteArray(_array)
                .AlignPatch(_alignment)
            ;
            for (var i = 0; i < _builders.Length; i++) _builders[i].Position = patchPosition + valueSize * i;
        }

        public class ValuePositionBuilder : IBuilder<TValue>
        {
            public void Build(IBlobStream stream) {}
            public int Position { get; internal set; }
        }
    }

    public class ArrayBuilder<TValue> : ArrayBuilder<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilder([NotNull] TValue[] array) : base(array) {}
    }
}