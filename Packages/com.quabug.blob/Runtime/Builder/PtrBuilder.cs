using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class PtrBuilder<TValue, TPtr> : Builder<TPtr>
        where TValue : unmanaged
        where TPtr : unmanaged
    {
        protected readonly IBuilder<TValue> _Builder;

        static PtrBuilder()
        {
            // HACK: assume `BlobPtr` has and only has an int `offset` field.
            if (sizeof(TPtr) != sizeof(int))
                throw new ArgumentException($"{nameof(TPtr)} must has and only has an int `Offset` field");
        }

        public PtrBuilder() => _Builder = new ValueBuilder<TValue>();
        public PtrBuilder(TValue value) => _Builder = new ValueBuilder<TValue>(value);
        public PtrBuilder([NotNull] IBuilder<TValue> builder) => _Builder = builder;

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            return _Builder.Build(stream, patchPosition, patchPosition);
        }
    }

    public class PtrBuilder<T> : PtrBuilder<T, BlobPtr<T>> where T : unmanaged
    {
        public PtrBuilder() {}
        public PtrBuilder(T value) : base(value) {}
        public PtrBuilder([NotNull] IBuilder<T> builder) : base(builder) {}
    }
}