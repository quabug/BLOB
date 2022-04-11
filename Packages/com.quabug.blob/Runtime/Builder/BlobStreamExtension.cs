using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public static class BlobStreamExtension
    {
        public static IBlobStream EnsureDataSize([NotNull] this IBlobStream stream, int size, int alignment)
        {
            var expectedPatchPosition = (int)Utilities.Align(stream.DataPosition + size, alignment);
            stream.PatchPosition = Math.Max(stream.PatchPosition, expectedPatchPosition);
            // expand stream buffer by patch position
            if (stream.Length < stream.PatchPosition) stream.Length = stream.PatchPosition;
            return stream;
        }

        public static unsafe IBlobStream EnsureDataSize<T>([NotNull] this IBlobStream stream) where T : unmanaged
        {
            return stream.EnsureDataSize(sizeof(T), Utilities.AlignOf<T>());
        }

        public static IBlobStream ExpandPatch([NotNull] this IBlobStream stream, int size, int alignment)
        {
            stream.PatchPosition = (int)Utilities.Align(stream.PatchPosition + size, alignment);
            return stream;
        }

        public static IBlobStream AlignPatch([NotNull] this IBlobStream stream, int alignment)
        {
            stream.PatchPosition = (int)Utilities.Align(stream.PatchPosition, alignment);
            return stream;
        }

        public static IBlobStream AlignPatch<T>([NotNull] this IBlobStream stream) where T : unmanaged
        {
            return stream.AlignPatch(Utilities.AlignOf<T>());
        }

        public static unsafe IBlobStream WriteValue<T>([NotNull] this IBlobStream stream, ref T value) where T : unmanaged
        {
            fixed (T* valuePtr = &value)
            {
                var size = sizeof(T);
                stream.Write((byte*)valuePtr, size, Utilities.AlignOf<T>());
            }
            return stream;
        }

        public static unsafe IBlobStream WriteValue<T>([NotNull] this IBlobStream stream, T value) where T : unmanaged
        {
            var valuePtr = &value;
            var size = sizeof(T);
            stream.Write((byte*)valuePtr, size, Utilities.AlignOf<T>());
            return stream;
        }

        public static IBlobStream WriteValue([NotNull] this IBlobStream stream, IBuilder builder)
        {
            builder.Build(stream);
            return stream;
        }

        public static IBlobStream WriteArray<T>([NotNull] this IBlobStream stream, T[] array, int alignment) where T : unmanaged
        {
            stream.WriteArrayMeta(array.Length);
            var dataPosition = stream.DataPosition;
            return stream.ToPatchPosition().WriteArrayData(array, alignment).ToPosition(dataPosition);
        }

        public static IBlobStream WriteArray<T>([NotNull] this IBlobStream stream, T[] array) where T : unmanaged
        {
            return stream.WriteArray(array, Utilities.AlignOf<T>());
        }

        public static unsafe IBlobStream WriteArray<T>(
            [NotNull] this IBlobStream stream,
            [NotNull, ItemNotNull] IReadOnlyList<IBuilder<T>> itemBuilders
        ) where T : unmanaged
        {
            return stream.WriteArray(itemBuilders, sizeof(T), Utilities.AlignOf<T>());
        }

        public static IBlobStream WriteArray(
            [NotNull] this IBlobStream stream,
            [NotNull, ItemNotNull] IReadOnlyList<IBuilder> itemBuilders,
            int itemSize,
            int alignment
        )
        {
            stream.WriteArrayMeta(itemBuilders.Count);
            var dataPosition = stream.DataPosition;
            return stream.ToPatchPosition().WriteArrayData(itemBuilders, itemSize, alignment).ToPosition(dataPosition);
        }

        public static IBlobStream WriteArrayMeta([NotNull] this IBlobStream stream, int length)
        {
            return stream.WritePatchOffset().WriteValue(length);
        }

        public static IBlobStream WriteArrayMeta([NotNull] this IBlobStream stream, int length, int patchOffset)
        {
            return stream.WriteValue(patchOffset).WriteValue(length);
        }

        public static IBlobStream WriteArrayData<T>([NotNull] this IBlobStream stream, T[] array) where T : unmanaged
        {
            return stream.WriteArrayData(array, Utilities.AlignOf<T>());
        }

        public static unsafe IBlobStream WriteArrayData<T>([NotNull] this IBlobStream stream, T[] array, int alignment) where T : unmanaged
        {
            if (array.Length == 0) return stream;
            var valueSize = sizeof(T);
            var arraySize = valueSize * array.Length;
            fixed (void* arrayPtr = &array[0]) stream.Write((byte*) arrayPtr, arraySize, alignment);
            return stream;
        }

        public static unsafe IBlobStream WriteArrayData<T>(
            [NotNull] this IBlobStream stream,
            [NotNull, ItemNotNull] IReadOnlyList<IBuilder<T>> itemBuilders
        ) where T : unmanaged
        {
            return stream.WriteArrayData(itemBuilders, sizeof(T), Utilities.AlignOf<T>());
        }

        public static IBlobStream WriteArrayData(
            [NotNull] this IBlobStream stream,
            [NotNull, ItemNotNull] IReadOnlyList<IBuilder> itemBuilders,
            int itemSize,
            int alignment
        )
        {
            var patchPosition = stream.PatchPosition;
            stream.ExpandPatch(itemSize * itemBuilders.Count, alignment);
            for (var i = 0; i < itemBuilders.Count; i++)
            {
                stream.DataPosition = patchPosition + itemSize * i;
                itemBuilders[i].Build(stream);
            }
            return stream;
        }

        public static IBlobStream WriteOffset([NotNull] this IBlobStream stream, int position)
        {
            var offset = position - stream.DataPosition;
            stream.WriteValue(offset);
            return stream;
        }

        public static IBlobStream WritePatchOffset([NotNull] this IBlobStream stream)
        {
            return stream.WriteOffset(stream.PatchPosition);
        }

        public static IBlobStream ToPatchPosition([NotNull] this IBlobStream stream)
        {
            return stream.ToPosition(stream.PatchPosition);
        }

        public static IBlobStream ToPosition([NotNull] this IBlobStream stream, int position)
        {
            stream.DataPosition = position;
            return stream;
        }
    }
}