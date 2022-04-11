﻿using System;
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

        internal ManagedBlobAssetReference([NotNull] byte[] blob)
        {
            if (blob.Length == 0) throw new ArgumentException("BLOB cannot be empty");
            _blob = blob;
        }
    }

    public unsafe class ManagedBlobAssetReference<T> where T : unmanaged
    {
        private readonly byte[] _blob;

        public ref T Value => ref *UnsafePtr;
        public T* UnsafePtr
        {
            get
            {
                fixed (void* ptr = &_blob[0]) return (T*)ptr;
            }
        }

        public int Length => _blob.Length;
        public byte[] Blob => _blob;

        internal ManagedBlobAssetReference([NotNull] byte[] blob)
        {
            if (blob.Length == 0) throw new ArgumentException("BLOB cannot be empty");
            _blob = blob;
        }
    }
}