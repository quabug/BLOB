using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

public interface IBlobBuilder
{
    /// <summary>
    /// serialize builder value into BLOB stream
    /// </summary>
    /// <param name="stream">BLOB stream</param>
    /// <param name="dataPosition">begin position of current data</param>
    /// <param name="patchPosition">begin position of patched(dynamic) data</param>
    /// <param name="alignment">data alignment</param>
    /// <returns>patch position after building</returns>
    int Build(Stream stream, int dataPosition, int patchPosition, int alignment = 0);
}

/// <summary>
/// compatible with Unity.Entities.BlobPtr
/// </summary>
/// <typeparam name="T"></typeparam>
public struct BlobPtr<T> where T : unmanaged
{
    internal int Offset;

    public unsafe ref T Value
    {
        get
        {
            // TODO: check validation
            fixed (void* thisPtr = &Offset)
            {
                return ref *(T*)((byte*)thisPtr + Offset);
            }
        }
    }
}

/// <summary>
/// compatible with Unity.Entities.BlobArray
/// </summary>
/// <typeparam name="T"></typeparam>
public struct BlobArray<T> where T : unmanaged
{
    internal int Offset;
    internal int Length;

    public unsafe ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException($"index({index}) out of range[0-{Length})");
            fixed (void* thisPtr = &Offset)
            {
                return ref *(T*)((byte*)thisPtr + index * sizeof(T));
            }
        }
    }
}

public unsafe class BlobBuilder<T> : IBlobBuilder where T : unmanaged
{
    private T _value;
    public ref T Value => ref _value;

    public BlobBuilder() {}
    public BlobBuilder(T value) => _value = value;

    private readonly Dictionary<int, IBlobBuilder> _fieldBuilderMap = new Dictionary<int, IBlobBuilder>();

    public BlobBuilder<T> SetBuilder<TField>(ref TField field, [NotNull] IBlobBuilder builder) where TField : unmanaged
    {
        fixed (void* valuePtr = &_value)
        fixed (void* fieldPtr = &field)
        {
            if (fieldPtr < valuePtr || fieldPtr >= (byte*)valuePtr + sizeof(T)) throw new ArgumentException("invalid field");
            _fieldBuilderMap[(int)((byte*)fieldPtr - (byte*)valuePtr)] = builder;
        }
        return this;
    }

    public int Build(Stream stream, int dataPosition, int patchPosition, int alignment = 0)
    {
        patchPosition = Math.Max(patchPosition, dataPosition + sizeof(T));
        foreach (var fieldInfo in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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

public unsafe class BlobPtrBuilder<T> : IBlobBuilder where T : unmanaged
{
    private BlobPtr<T> _blobPtr;
    private readonly IBlobBuilder _builder;

    public BlobPtrBuilder() => _builder = new BlobBuilder<T>();
    public BlobPtrBuilder(T value) => _builder = new BlobBuilder<T>(value);
    public BlobPtrBuilder(IBlobBuilder builder) => _builder = builder;

    public int Build(Stream stream, int dataPosition, int patchPosition, int alignment = 0)
    {
        patchPosition = Math.Max(patchPosition, dataPosition + sizeof(BlobPtr<T>));
        _blobPtr.Offset = patchPosition - dataPosition;
        stream.Seek(dataPosition, SeekOrigin.Begin);
        stream.WriteValue(ref _blobPtr);
        return _builder.Build(stream, patchPosition, patchPosition);
    }
}

public class BlobArrayBuilder<T> : IBlobBuilder where T : unmanaged
{
    private BlobArray<T> _blobArray;
    private readonly IBlobBuilder[] _builders;

    public BlobArrayBuilder(IEnumerable<T> builders) =>
        _builders = builders.Select(value => (IBlobBuilder)new BlobBuilder<T>(value)).ToArray();

    public BlobArrayBuilder(IEnumerable<IBlobBuilder> builders) => _builders = builders.ToArray();

    public unsafe int Build(Stream stream, int dataPosition, int patchPosition, int alignment = 0)
    {
        patchPosition = Math.Max(patchPosition, dataPosition + sizeof(BlobArray<T>));
        _blobArray.Offset = patchPosition - dataPosition;
        var length = _builders.Length;
        _blobArray.Length = length;
        stream.Seek(dataPosition, SeekOrigin.Begin);
        stream.WriteValue(ref _blobArray);
        var arrayPatchPosition = patchPosition + sizeof(T) * length;
        for (var i = 0; i < length; i++)
        {
            var arrayDataPosition = patchPosition + sizeof(T) * i;
            arrayPatchPosition = _builders[i].Build(stream, arrayDataPosition, arrayPatchPosition);
        }
        return arrayPatchPosition;
    }
}

public static class StreamExtension
{
    public static unsafe void WriteValue<T>(this Stream stream, ref T value) where T : unmanaged
    {
        fixed (T* valuePtr = &value)
        {
            var size = sizeof(T);
            WriteValuePtr(stream, (byte*)valuePtr, size);
        }
    }

    public static unsafe void WriteValuePtr(this Stream stream, byte* valuePtr, int size)
    {
        // TODO: should handle endianness?
        for (var i = 0; i < size; i++) stream.WriteByte(*(valuePtr + i));
    }

    public static long SeekFromBegin(this Stream stream, int position, int alignment = 0)
    {
        position = Utilities.Align(position, alignment);
        return stream.Seek(position, SeekOrigin.Begin);
    }
}

public static class Utilities
{
    public static long Align(long address, int alignment = 0)
    {
        if (alignment <= 0) alignment = IntPtr.Size;
        return (address + (alignment - 1)) & -alignment;
    }

    public static int Align(int address, int alignment)
    {
        return (int)Align((long)address, alignment);
    }
}