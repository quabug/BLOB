using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class PtrBuilderWithNewValue<TValue, TPtr> : Builder<TPtr>
        where TValue : unmanaged
        where TPtr : unmanaged
    {
        [NotNull] private readonly IBuilder<TValue> _builder;
        public IBuilder<TValue> ValueBuilder => _builder;

        static PtrBuilderWithNewValue()
        {
            // HACK: assume `BlobPtr` has and only has an int `offset` field.
            if (sizeof(TPtr) != sizeof(int))
                throw new ArgumentException($"{nameof(TPtr)} must has and only has an int `Offset` field");
        }

        public PtrBuilderWithNewValue() => _builder = new ValueBuilder<TValue>();
        public PtrBuilderWithNewValue(TValue value) => _builder = new ValueBuilder<TValue>(value);
        public PtrBuilderWithNewValue([NotNull] IBuilder<TValue> builder) => _builder = builder;

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(patchPosition - dataPosition);
            stream.WriteValue(ref offset);
            return _builder.Build(stream, patchPosition, patchPosition);
        }
    }

    public class PtrBuilderWithNewValue<T> : PtrBuilderWithNewValue<T, BlobPtr<T>> where T : unmanaged
    {
        public PtrBuilderWithNewValue() {}
        public PtrBuilderWithNewValue(T value) : base(value) {}
        public PtrBuilderWithNewValue([NotNull] IBuilder<T> builder) : base(builder) {}
    }
}