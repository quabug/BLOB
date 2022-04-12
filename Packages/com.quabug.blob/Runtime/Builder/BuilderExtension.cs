using System.Collections.Generic;
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

        [NotNull]
        public static ManagedBlobAssetReference CreateManagedBlobAssetReference([NotNull] this IBuilder builder)
        {
            return builder.CreateManagedBlobAssetReference(1);
        }

        [NotNull] public static ManagedBlobAssetReference CreateManagedBlobAssetReference([NotNull] this IBuilder builder, int alignment)
        {
            return new ManagedBlobAssetReference(builder.CreateBlob(alignment));
        }

        [NotNull]
        public static byte[] CreateBlob([NotNull] this IBuilder builder)
        {
            return builder.CreateBlob(1);
        }

        [NotNull] public static byte[] CreateBlob([NotNull] this IBuilder builder, int alignment)
        {
            using var stream = new BlobMemoryStream();
            builder.Build(stream);
            stream.Length = (int)Utilities.Align(stream.Length, alignment);
            return stream.ToArray();
        }

        [NotNull] public static byte[] CreateBlob<T>([NotNull] this IBuilder<T> builder) where T : unmanaged
        {
            using var stream = new BlobMemoryStream();
            builder.Build(stream);
            stream.Length = (int)Utilities.Align<T>(stream.Length);
            return stream.ToArray();
        }

#region SetPointer of StructBuilder
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
#endregion

#region SetArray of StructBuilder
        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            return builder.SetArray(ref field, items, Utilities.AlignOf<TValue>());
        }

        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items,
            int alignment
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            return builder.SetArray(ref field, items.ToArray(), alignment);
        }

        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] TValue[] items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            return builder.SetArray(ref field, items, Utilities.AlignOf<TValue>());
        }

        [NotNull] public static ArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobArray<TValue> field,
            [NotNull] TValue[] items,
            int alignment
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new ArrayBuilder<TValue>(items, alignment);
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
#endregion

#region SetValue of StructBuilder
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
#endregion

#region SetString of StructBuilder
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
#endregion

#region SetTree of StructBuilder
        [NotNull] public static TreeBuilder<TValue> SetTree<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobTree<TValue> field,
            [NotNull] ITreeNode<TValue> root
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            return builder.SetTree(ref field, root, Utilities.AlignOf<TValue>());
        }

        [NotNull] public static TreeBuilder<TValue> SetTree<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobTree<TValue> field,
            [NotNull] ITreeNode<TValue> root,
            int alignment
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var treeBuilder = new TreeBuilder<TValue>(root, alignment);
            builder.SetBuilder(ref field, treeBuilder);
            return treeBuilder;
        }

        [NotNull] public static TreeBuilder<TValue> SetTree<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref BlobTree<TValue> field,
            [NotNull] ITreeNode<IBuilder<TValue>> root
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var treeBuilder = new TreeBuilder<TValue>(root);
            builder.SetBuilder(ref field, treeBuilder);
            return treeBuilder;
        }
#endregion
    }
}