using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class BlobPtrBuilder<TValue, TPtr> : IBlobBuilder<TPtr>
        where TValue : unmanaged
        where TPtr : unmanaged
    {
        protected readonly IBlobBuilder<TValue> _Builder;

        static BlobPtrBuilder()
        {
            // HACK: assume `BlobPtr` has and only has an int `offset` field.
            if (sizeof(TPtr) != sizeof(int))
                throw new ArgumentException($"{nameof(TPtr)} must has and only has an int `Offset` field");
        }

        public BlobPtrBuilder() => _Builder = new BlobBuilder<TValue>();
        public BlobPtrBuilder(TValue value) => _Builder = new BlobBuilder<TValue>(value);
        public BlobPtrBuilder([NotNull] IBlobBuilder<TValue> builder) => _Builder = builder;

        public virtual long Build(Stream stream, long dataPosition, long patchPosition)
        {
            patchPosition = Utilities.EnsurePatchPosition<TPtr>(patchPosition, dataPosition);
            var offset = (int)(patchPosition - dataPosition);
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            return _Builder.Build(stream, patchPosition, patchPosition);
        }
    }

    public class BlobPtrBuilder<T> : BlobPtrBuilder<T, BlobPtr<T>> where T : unmanaged
    {
        public BlobPtrBuilder() {}
        public BlobPtrBuilder(T value) : base(value) {}
        public BlobPtrBuilder([NotNull] IBlobBuilder<T> builder) : base(builder) {}
    }
}