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
            return stream;
        }

        public static IBlobStream ReservePatch([NotNull] this IBlobStream stream, int size, int alignment)
        {
            stream.PatchPosition = (int)Utilities.Align(stream.PatchPosition + size, alignment);
            return stream;
        }

        public static unsafe IBlobStream EnsureDataSize<T>([NotNull] this IBlobStream stream) where T : unmanaged
        {
            return stream.EnsureDataSize(sizeof(T), Utilities.AlignOf<T>());
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

        public static IBlobStream WriteValue<T>([NotNull] this IBlobStream stream, IBuilder<T> builder) where T : unmanaged
        {
            builder.Build(stream);
            return stream;
        }

        public static unsafe IBlobStream WriteArray<T>([NotNull] this IBlobStream stream, T[] array) where T : unmanaged
        {
            if (array.Length == 0) return stream;
            var valueSize = sizeof(T);
            var arraySize = valueSize * array.Length;
            fixed (void* arrayPtr = &array[0]) stream.Write((byte*) arrayPtr, arraySize, Utilities.AlignOf<T>());
            return stream;
        }

        public static unsafe IBlobStream WriteArray<T>(
            [NotNull] this IBlobStream stream,
            [NotNull, ItemNotNull] IReadOnlyList<IBuilder<T>> itemBuilders
        ) where T : unmanaged
        {
            var patchPosition = stream.PatchPosition;
            stream.ReservePatch(sizeof(T) * itemBuilders.Count, Utilities.AlignOf<T>());
            for (var i = 0; i < itemBuilders.Count; i++)
            {
                stream.DataPosition = patchPosition + sizeof(T) * i;
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