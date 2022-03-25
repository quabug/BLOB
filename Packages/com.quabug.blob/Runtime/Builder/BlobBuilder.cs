using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class BlobBuilder<T> : IBlobBuilder<T> where T : unmanaged
    {
        private T _value;
        public ref T Value => ref _value;

        public BlobBuilder() {}
        public BlobBuilder(T value) => _value = value;

        private readonly Dictionary<int, IBlobBuilder> _fieldBuilderMap = new Dictionary<int, IBlobBuilder>();

        public BlobBuilder<T> SetBuilder<TField>(ref TField field, [NotNull] IBlobBuilder<TField> builder)
            where TField : unmanaged
        {
            fixed (void* valuePtr = &_value)
            fixed (void* fieldPtr = &field)
            {
                if (fieldPtr < valuePtr || fieldPtr >= (byte*)valuePtr + sizeof(T))
                    throw new ArgumentException("invalid field");
                _fieldBuilderMap[(int)((byte*)fieldPtr - (byte*)valuePtr)] = builder;
            }

            return this;
        }

        public long Build([NotNull] Stream stream, long dataPosition, long patchPosition)
        {
            patchPosition = Utilities.EnsurePatchPosition<T>(patchPosition, dataPosition);

            const BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var fieldInfo in typeof(T).GetFields(fieldFlags))
            {
                // HACK: Marshal.Offset returns invalid value on IL2CPP back-end of Unity.
#if ENABLE_IL2CPP
                var fieldOffset = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.GetFieldOffset(fieldInfo);
#else
                var fieldOffset = Marshal.OffsetOf<T>(fieldInfo.Name).ToInt32();
#endif
                if (_fieldBuilderMap.TryGetValue(fieldOffset, out var builder))
                {
                    // TODO: restrict on writing-size of field value?
                    patchPosition = builder.Build(stream, dataPosition + fieldOffset, patchPosition);
                }
                else
                {
                    stream.Seek(dataPosition + fieldOffset, SeekOrigin.Begin);
                    fixed (void* valuePtr = &_value)
                    {
                        var fieldPtr = (byte*)valuePtr + fieldOffset;
                        var fieldSize = Marshal.SizeOf(fieldInfo.FieldType);
                        stream.WriteValuePtr(fieldPtr, fieldSize);
                    }
                }
            }

            return patchPosition;
        }
    }

    public static partial class BlobExtension
    {
        public static ManagedBlobAssetReference<T> CreateManagedBlobAssetReference<T>(this IBlobBuilder<T> builder) where T : unmanaged
        {
            return new ManagedBlobAssetReference<T>(builder.CreateBlob());
        }

        public static byte[] CreateBlob<T>(this IBlobBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream);
            stream.SetLength(Utilities.Align<T>(stream.Length));
            return stream.ToArray();
        }

        public static Stream CreateBlob<T>(this IBlobBuilder<T> builder, Stream stream) where T : unmanaged
        {
            builder.Build(stream, 0, 0);
            return stream;
        }
    }

    public static class StreamExtension
    {
        public static unsafe void WriteValue<T>([NotNull] this Stream stream, ref T value) where T : unmanaged
        {
            fixed (T* valuePtr = &value)
            {
                var size = sizeof(T);
                WriteValuePtr(stream, (byte*)valuePtr, size);
            }
        }

        public static unsafe void WriteValuePtr([NotNull] this Stream stream, byte* valuePtr, int size)
        {
            // TODO: should handle endianness?
            for (var i = 0; i < size; i++) stream.WriteByte(*(valuePtr + i));
        }
    }

    public static class Utilities
    {
        public static long Align(long address)
        {
            return Align(address, IntPtr.Size);
        }

        public static long Align<T>(long address) where T : unmanaged
        {
            return Align(address, AlignOf<T>());
        }

        public static long Align(long address, int alignment)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), "alignment must be greater than 0");
            if (!PowerOfTwo(alignment))
                throw new ArgumentOutOfRangeException(nameof(alignment), "alignment must be power of 2");
            return (address + (alignment - 1)) & -alignment;

            static bool PowerOfTwo(int n)
            {
                return (n & (n - 1)) == 0;
            }
        }

        public static unsafe int AlignOf<T>() where T : unmanaged => sizeof(AlignHelper<T>) - sizeof(T);

        struct AlignHelper<T> where T : unmanaged
        {
            public byte _;
            public T Data;
        }

        public static unsafe long EnsurePatchPosition<T>(long patchPosition, long dataPosition) where T : unmanaged
        {
            return Align<T>(Math.Max(patchPosition, dataPosition + sizeof(T)));
        }
    }
}