#if UNITY_BLOB

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Unity.Entities;

namespace Blob
{
    public static partial class BuilderExtension
    {
        public static BlobAssetReference<T> CreateUnityBlobAssetReference<T>([NotNull] this IBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream);
            // expand stream to 16-bytes-aligned length as same as Unity BLOB
            var alignedLength = Utilities.Align(stream.Length, 16);
            stream.SetLength(alignedLength);
            return BlobAssetReference<T>.Create(stream.ToArray());
        }

        public static UnityBlobPtrBuilderWithRefBuilder<TValue> SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobPtr<TValue> field,
            [NotNull] IBuilder<TValue> refBuilder
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var ptrBuilder = new UnityBlobPtrBuilderWithRefBuilder<TValue>(refBuilder);
            builder.SetBuilder(ref field, ptrBuilder);
            return ptrBuilder;
        }

        public static UnityBlobPtrBuilderWithNewValue<TValue> SetPointer<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobPtr<TValue> field,
            TValue value
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var ptrBuilder = new UnityBlobPtrBuilderWithNewValue<TValue>(value);
            builder.SetBuilder(ref field, ptrBuilder);
            return ptrBuilder;
        }

        public static UnityBlobArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobArray<TValue> field,
            [NotNull] IEnumerable<TValue> items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new UnityBlobArrayBuilder<TValue>(items);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        public static UnityBlobArrayBuilder<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobArray<TValue> field,
            [NotNull] TValue[] items
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new UnityBlobArrayBuilder<TValue>(items);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        public static UnityBlobArrayBuilderWithItemBuilders<TValue> SetArray<T, TValue>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobArray<TValue> field,
            [NotNull] IEnumerable<IBuilder<TValue>> itemBuilders
        )
            where T : unmanaged
            where TValue : unmanaged
        {
            var arrayBuilder = new UnityBlobArrayBuilderWithItemBuilders<TValue>(itemBuilders);
            builder.SetBuilder(ref field, arrayBuilder);
            return arrayBuilder;
        }

        public static UnityBlobStringBuilder SetString<T>(
            [NotNull] this StructBuilder<T> builder,
            ref Unity.Entities.BlobString field,
            string value
        )
            where T : unmanaged
        {
            var stringBuilder = new UnityBlobStringBuilder(value);
            builder.SetBuilder(ref field, stringBuilder);
            return stringBuilder;
        }
    }
}

#endif