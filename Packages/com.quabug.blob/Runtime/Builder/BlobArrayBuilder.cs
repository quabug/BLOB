using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class BlobArrayBuilder<TValue, TArray> : IBlobBuilder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly IBlobBuilder<TValue>[] _builders;

        static BlobArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public BlobArrayBuilder() => _builders = Array.Empty<IBlobBuilder<TValue>>();

        public BlobArrayBuilder([NotNull] IEnumerable<TValue> elements) =>
            _builders = elements.Select(value => (IBlobBuilder<TValue>)new ElementBuilder(value)).ToArray();

        public BlobArrayBuilder([NotNull, ItemNotNull] IEnumerable<IBlobBuilder<TValue>> builders) => _builders = builders.ToArray();

        public virtual long Build([NotNull] Stream stream, long dataPosition, long patchPosition)
        {
            patchPosition = Utilities.EnsurePatchPosition<TArray>(patchPosition, dataPosition);
            var offset = (int)(patchPosition - dataPosition);
            var length = _builders.Length;
            stream.Seek(dataPosition, SeekOrigin.Begin);
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

        public class ElementBuilder : IBlobBuilder<TValue>
        {
            private TValue _value;
            public ElementBuilder(TValue value) => _value = value;
            public long Build(Stream stream, long dataPosition, long patchPosition)
            {
                stream.Seek(dataPosition, SeekOrigin.Begin);
                stream.WriteValue(ref _value);
                return patchPosition;
            }
        }
    }

    public class BlobArrayBuilder<T> : BlobArrayBuilder<T, BlobArray<T>> where T : unmanaged
    {
        public BlobArrayBuilder() {}
        public BlobArrayBuilder([NotNull] IEnumerable<T> elements) : base(elements) {}
        public BlobArrayBuilder([NotNull, ItemNotNull] IEnumerable<IBlobBuilder<T>> builders) : base(builders) {}
    }
}