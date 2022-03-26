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

        public static RefPtrBuilder<TValue> SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobPtr<TValue> field,
            [NotNull] IBuilder<TValue> refBuilder
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var refPtrBuilder = new RefPtrBuilder<TValue>(refBuilder);
            builder.SetBuilder(ref field, refPtrBuilder);
            return refPtrBuilder;
        }

        public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new ArrayBuilder<TValue>(items);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        public static ValueBuilder<TField> SetValue<T, TField>(
            [NotNull] this StructBuilder<T> builder,
            ref TField field,
            TField value
        )
            where T : unmanaged
            where TField : unmanaged
        {
            var valueBuilder = new ValueBuilder<TField>(value);
            builder.SetBuilder(ref field, valueBuilder);
            field = value;
            return valueBuilder;
        }
    }
}