using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public static partial class BuilderExtension
    {
        public static ManagedBlobAssetReference<T> CreateManagedBlobAssetReference<T>(this IBuilder<T> builder) where T : unmanaged
        {
            return new ManagedBlobAssetReference<T>(builder.CreateBlob());
        }

        public static byte[] CreateBlob<T>(this IBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream);
            stream.SetLength(Utilities.Align<T>(stream.Length));
            return stream.ToArray();
        }

        public static Stream CreateBlob<T>(this IBuilder<T> builder, Stream stream) where T : unmanaged
        {
            builder.Build(stream, 0, 0);
            return stream;
        }

        public static void SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobPtr<TValue> field,
            [NotNull] IBuilder<TValue> refBuilder
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            builder.SetBuilder(ref field, new RefPtrBuilder<TValue>(refBuilder));
        }

        public static void SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            builder.SetBuilder(ref field, new ArrayBuilder<TValue>(items));
        }
    }
}