using System;
using System.Text;

namespace Blob
{
    /// <summary>
    /// compatible with Unity.Entities.BlobPtr
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe struct BlobPtr<T> where T : unmanaged
    {
        internal int Offset;

        public ref T Value
        {
            get
            {
                fixed (void* ptr = &Offset)
                {
                    return ref *(T*)((byte*)ptr + Offset);
                }
            }
        }
    }

    public unsafe struct BlobArray<T> where T : unmanaged
    {
        internal int Offset;
        private int _length;
        public int Length { get => _length; internal set => _length = value; }

        public ref T this[int index]
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

        public T* GetUnsafePtr()
        {
            fixed (void* thisPtr = &Offset)
            {
                return (T*)((byte*) thisPtr + Offset);
            }
        }
    }

    public unsafe struct BlobString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public new string ToString() => new TEncoding().GetString(Data.GetUnsafePtr(), Data.Length);
    }

    public unsafe struct BlobNullTerminatedString<TEncoding> where TEncoding : Encoding, new()
    {
        internal BlobArray<byte> Data;
        public new string ToString() => new TEncoding().GetString(Data.GetUnsafePtr(), Data.Length-1);
    }

    public struct BlobStringUnity
    {
        internal BlobNullTerminatedString<UTF8Encoding> Data;
        public new string ToString() => Data.ToString();
    }

    public unsafe class ManagedBlobAssetReference<T> where T : unmanaged
    {
        private byte[] _blob;
        private readonly T* _value;

        public ref T Value => ref *_value;

        internal ManagedBlobAssetReference(byte[] blob)
        {
            _blob = blob;
            fixed (void* ptr = &blob[0]) _value = (T*)ptr;
        }
    }
}