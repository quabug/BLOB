using System;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class RefPtrBuilder<TValue, TPtr> : Builder<TPtr>
        where TValue : unmanaged
        where TPtr : unmanaged
    {
        [NotNull] private readonly IBuilder<TValue> _refBuilder;

        static RefPtrBuilder()
        {
            // HACK: assume `BlobPtr` has and only has an int `offset` field.
            if (sizeof(TPtr) != sizeof(int))
                throw new ArgumentException($"{nameof(TPtr)} must has and only has an int `Offset` field");
        }

        public RefPtrBuilder([NotNull] IBuilder<TValue> refBuilder)
        {
            _refBuilder = refBuilder;
        }

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            var offset = (int)(_refBuilder.Position - dataPosition);
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            return patchPosition;
        }
    }

    public class RefPtrBuilder<T> : RefPtrBuilder<T, BlobPtr<T>> where T : unmanaged
    {
        public RefPtrBuilder([NotNull] IBuilder<T> builder) : base(builder) {}
    }
}