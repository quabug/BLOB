using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public static partial class BuilderExtension
    {
        [NotNull] public static ManagedBlobAssetReference<T> CreateManagedBlobAssetReference<T>([NotNull] this IBuilder<T> builder) where T : unmanaged
        {
            return new ManagedBlobAssetReference<T>(builder.CreateBlob());
        }

        [NotNull] public static byte[] CreateBlob<T>([NotNull] this IBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream);
            stream.SetLength(Utilities.Align<T>(stream.Length));
            return stream.ToArray();
        }

        [NotNull] public static Stream CreateBlob<T>([NotNull] this IBuilder<T> builder, [NotNull] Stream stream) where T : unmanaged
        {
            builder.Build(stream, 0, 0);
            return stream;
        }

        [NotNull] public static PtrBuilderWithRefBuilder<TValue> SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobPtr<TValue> field,
            [NotNull] IBuilder<TValue> refBuilder
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var ptrBuilder = new PtrBuilderWithRefBuilder<TValue>(refBuilder);
            builder.SetBuilder(ref field, ptrBuilder);
            return ptrBuilder;
        }

        [NotNull] public static PtrBuilderWithNewValue<TValue> SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobPtr<TValue> field,
            TValue value
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var ptrBuilder = new PtrBuilderWithNewValue<TValue>(value);
            builder.SetBuilder(ref field, ptrBuilder);
            return ptrBuilder;
        }

        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new ArrayBuilder<TValue>(items.ToArray());
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] TValue[] items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new ArrayBuilder<TValue>(items);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        [NotNull] public static ArrayBuilderWithItemBuilders<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<IBuilder<TValue>> itemBuilders
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new ArrayBuilderWithItemBuilders<TValue>(itemBuilders);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        [NotNull] public static ValueBuilder<TField> SetValue<T, TField>(
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

        [NotNull] public static StringBuilder<TEncoding> SetString<T, TEncoding>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobString<TEncoding> field,
            [NotNull] string value
        )
            where T : unmanaged
            where TEncoding : Encoding, new()
        {
            var stringBuilder = new StringBuilder<TEncoding>(value);
            builder.SetBuilder(ref field, stringBuilder);
            return stringBuilder;
        }
    }
}