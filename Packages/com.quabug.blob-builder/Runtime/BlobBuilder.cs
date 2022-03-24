using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public interface IBlobBuilder
    {
        /// <summary>
        /// serialize builder value into BLOB stream
        /// </summary>
        /// <param name="stream">BLOB stream</param>
        /// <param name="dataPosition">begin position of current data</param>
        /// <param name="patchPosition">begin position of patched(dynamic) data</param>
        /// <returns>patch position after building</returns>
        long Build(Stream stream, long dataPosition, long patchPosition);
    }

    public interface IBlobBuilder<T> : IBlobBuilder where T : unmanaged
    {
    }

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
                var fieldOffset = Marshal.OffsetOf<T>(fieldInfo.Name).ToInt32();
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

    public unsafe class BlobArrayBuilder<TValue, TArray> : IBlobBuilder<TArray>
        where TValue : unmanaged
        where TArray : unmanaged
    {
        private readonly IBlobBuilder<TValue>[] _builders;

        static BlobArrayBuilder()
        {
            // HACK: assume `BlobArray` has and only has an int `offset` field and an int `length` field.
            if (sizeof(TArray) != (sizeof(int) + sizeof(int)))
                throw new ArgumentException($"{nameof(TArray)} must has and only has an int `Offset` field and an int `Length` field");
        }

        public BlobArrayBuilder() => _builders = Array.Empty<IBlobBuilder<TValue>>();

        public BlobArrayBuilder([NotNull] IEnumerable<TValue> elements) =>
            _builders = elements.Select(value => (IBlobBuilder<TValue>)new BlobBuilder<TValue>(value)).ToArray();

        public BlobArrayBuilder([NotNull, ItemNotNull] IEnumerable<IBlobBuilder<TValue>> builders) => _builders = builders.ToArray();

        public virtual long Build([NotNull] Stream stream, long dataPosition, long patchPosition)
        {
            patchPosition = Utilities.EnsurePatchPosition<TArray>(patchPosition, dataPosition);
            var offset = (int)(patchPosition - dataPosition);
            var length = _builders.Length;
            stream.Seek(dataPosition, SeekOrigin.Begin);
            stream.WriteValue(ref offset);
            stream.WriteValue(ref length);
            var arrayPatchPosition = Utilities.Align<TValue>(patchPosition + sizeof(TValue) * length);
            for (var i = 0; i < length; i++)
            {
                var arrayDataPosition = patchPosition + sizeof(TValue) * i;
                arrayPatchPosition = _builders[i].Build(stream, arrayDataPosition, arrayPatchPosition);
            }

            return arrayPatchPosition;
        }
    }

    public class BlobArrayBuilder<T> : BlobArrayBuilder<T, BlobArray<T>> where T : unmanaged
    {
        public BlobArrayBuilder() {}
        public BlobArrayBuilder([NotNull] IEnumerable<T> elements) : base(elements) {}
        public BlobArrayBuilder([NotNull, ItemNotNull] IEnumerable<IBlobBuilder<T>> builders) : base(builders) {}
    }

    public class BlobStringBuilder<TEncoding> : BlobArrayBuilder<byte, BlobString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public BlobStringBuilder() {}
        // TODO: optimize?
        public BlobStringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str)) {}
    }

    public class BlobNullTerminatedStringBuilder<TEncoding> : BlobArrayBuilder<byte, BlobNullTerminatedString<TEncoding>>
        where TEncoding : Encoding, new()
    {
        public BlobNullTerminatedStringBuilder() : base(new byte[] { 0 }) {}
        // TODO: optimize?
        public BlobNullTerminatedStringBuilder([NotNull] string str) : base(new TEncoding().GetBytes(str).Append((byte)0)) {}
    }

    public static partial class BlobExtension
    {
        public static ManagedBlobAssetReference<T> CreateManagedBlobAssetReference<T>(this IBlobBuilder<T> builder, int alignment = 0) where T : unmanaged
        {
            return new ManagedBlobAssetReference<T>(builder.CreateBlob(alignment));
        }

        public static byte[] CreateBlob<T>(this IBlobBuilder<T> builder, int alignment = 0) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream, alignment);
            return stream.ToArray();
        }

        public static Stream CreateBlob<T>(this IBlobBuilder<T> builder, Stream stream, int alignment = 0) where T : unmanaged
        {
            if (alignment <= 0) alignment = IntPtr.Size;
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