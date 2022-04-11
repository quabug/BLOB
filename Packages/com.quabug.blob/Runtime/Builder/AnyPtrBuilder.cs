﻿using System;

namespace Blob
{
    public class AnyPtrBuilder<T> : Builder<BlobAnyPtr> where T : unmanaged
    {
        public T Value { get; set; }

        public AnyPtrBuilder() {}
        public AnyPtrBuilder(T value) => Value = value;

        protected override unsafe void BuildImpl(IBlobStream stream)
        {
            stream.EnsureDataSize<BlobAnyPtr>()
                .WritePatchOffset()
                .WriteValue(sizeof(T))
                .ToPatchPosition()
                .WriteValue(Value)
                .AlignPatch(Utilities.AlignOf<T>())
            ;
        }
    }

    public class AnyPtrBuilder : Builder<BlobAnyPtr>
    {
        private byte[] _bytes;
        private int _alignment;

        public unsafe void SetValue<T>(T value) where T : unmanaged
        {
            _alignment = Utilities.AlignOf<T>();
            var size = sizeof(T);
            _bytes = new byte[size];
            if (size == 0) return;
            fixed (void* ptr = &_bytes[0])
            {
                Buffer.MemoryCopy(&value, ptr, size, size);
            }
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            stream.EnsureDataSize<BlobAnyPtr>()
                .WritePatchOffset()
                .WriteValue(_bytes.Length)
                .ToPatchPosition()
                .WriteArray(_bytes)
                .AlignPatch(_alignment)
            ;
        }
    }
}