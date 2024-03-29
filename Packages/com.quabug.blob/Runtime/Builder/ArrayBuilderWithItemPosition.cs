﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class ArrayBuilderWithItemPosition<TValue, TArray> : ArrayBuilder<TValue, TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly ValuePositionBuilder<TValue>[] _builders;

        public IBuilder<TValue> this[int index] => _builders[index];

        public ArrayBuilderWithItemPosition() : this(Array.Empty<TValue>()) {}
        public ArrayBuilderWithItemPosition([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilderWithItemPosition([NotNull] TValue[] array) : base(array)
        {
            _builders = new ValuePositionBuilder<TValue>[array.Length];
            for (var i = 0; i < _builders.Length; i++) _builders[i] = new ValuePositionBuilder<TValue>();
        }

        protected override void BuildImpl(IBlobStream stream, ref TArray data)
        {
            var valueSize = sizeof(TValue);
            base.BuildImpl(stream, ref data);
            for (var i = 0; i < _builders.Length; i++)
            {
                var builder = _builders[i];
                builder.DataPosition = PatchPosition + valueSize * i;
                builder.DataSize = valueSize;
                builder.PatchPosition = PatchPosition + PatchSize;
                builder.PatchSize = 0;
            }
        }
    }

    public class ArrayBuilderWithItemPosition<TValue> : ArrayBuilderWithItemPosition<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilderWithItemPosition([NotNull] TValue[] array) : base(array) {}
    }
}
