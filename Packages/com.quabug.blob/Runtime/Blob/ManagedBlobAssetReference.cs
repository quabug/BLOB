using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class ManagedBlobAssetReference
    {
        private readonly byte[] _blob;

        public ref T GetValue<T>() where T : unmanaged => ref *GetUnsafePtr<T>();
        public T* GetUnsafePtr<T>() where T : unmanaged
        {
            if (_blob.Length < sizeof(T)) throw new ArgumentException("invalid generic parameter");
            fixed (void* ptr = &_blob[0]) return (T*)ptr;
        }

        public int Length => _blob.Length;
        public byte[] Blob => _blob;

        public ManagedBlobAssetReference([NotNull] byte[] blob)
        {
            if (blob.Length == 0) throw new ArgumentException("BLOB cannot be empty");
            _blob = blob;
        }
    }

    public unsafe class ManagedBlobAssetReference<T> : IDisposable where T : unmanaged
    {
        private readonly byte[] _blob;
        private GCHandle _handle;

        public ref T Value => ref *UnsafePtr;
        public T* UnsafePtr => (T*)_handle.AddrOfPinnedObject().ToPointer();

        public int Length => _blob.Length;
        public byte[] Blob => _blob;

        public ManagedBlobAssetReference([NotNull] byte[] blob)
        {
            if (blob.Length == 0) throw new ArgumentException("BLOB cannot be empty");
            _blob = blob;
            _handle = GCHandle.Alloc(_blob, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            _handle.Free();
        }
    }
}