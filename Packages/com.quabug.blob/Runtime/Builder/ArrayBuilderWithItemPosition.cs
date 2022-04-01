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
        private readonly ValuePositionBuilder[] _builders;

        public IBuilder<TValue> this[int index] => _builders[index];

        public ArrayBuilderWithItemPosition() : this(Array.Empty<TValue>()) {}
        public ArrayBuilderWithItemPosition([NotNull] IEnumerable<TValue> items) : this(items.ToArray()) {}
        public ArrayBuilderWithItemPosition([NotNull] IEnumerable<TValue> items, int alignment) : this(items.ToArray(), alignment) {}
        public ArrayBuilderWithItemPosition([NotNull] TValue[] array) : this(array, Utilities.AlignOf<TValue>()) {}
        public ArrayBuilderWithItemPosition([NotNull] TValue[] array, int alignment) : base(array, alignment)
        {
            _builders = new ValuePositionBuilder[array.Length];
            for (var i = 0; i < _builders.Length; i++) _builders[i] = new ValuePositionBuilder();
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            var patchPosition = stream.PatchPosition;
            var valueSize = sizeof(TValue);
            base.BuildImpl(stream);
            for (var i = 0; i < _builders.Length; i++) _builders[i].Position = patchPosition + valueSize * i;
        }

        public class ValuePositionBuilder : IBuilder<TValue>
        {
            public void Build(IBlobStream stream)
            {
                // this builder is only made for record Position
                // so no build process here
            }

            public int Position { get; internal set; }
        }
    }

    public class ArrayBuilderWithItemPosition<TValue> : ArrayBuilderWithItemPosition<TValue, BlobArray<TValue>> where TValue : unmanaged
    {
        public ArrayBuilderWithItemPosition([NotNull] TValue[] array) : base(array) {}
    }
}
