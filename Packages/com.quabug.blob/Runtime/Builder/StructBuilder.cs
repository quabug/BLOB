using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe class StructBuilder<T> : Builder<T> where T : unmanaged
    {
        private T _value;
        public ref T Value => ref _value;

        private readonly List<(int offset, IBuilder builder)> _builders = new List<(int, IBuilder)>();
        private readonly Dictionary<int, IBuilder> _fieldBuilderMap = new Dictionary<int, IBuilder>();

        public void SetBuilder<TField>(ref TField field, [NotNull] IBuilder<TField> builder) where TField : unmanaged
        {
            var fieldOffset = GetFieldOffset(ref field);
            _fieldBuilderMap[fieldOffset] = builder;
            _builders.Add((fieldOffset, builder));
        }

        public void SetBuilder<TField>(ref TField field, TField value) where TField : unmanaged
        {
            SetBuilder(ref field, new ValueBuilder<TField>(value));
            field = value;
        }

        public IBuilder<TField> GetBuilder<TField>(ref TField field) where TField : unmanaged
        {
            var fieldOffset = GetFieldOffset(ref field);
            return (IBuilder<TField>)_fieldBuilderMap[fieldOffset];
        }

        private int GetFieldOffset<TField>(ref TField field) where TField : unmanaged
        {
            fixed (void* valuePtr = &_value)
            fixed (void* fieldPtr = &field)
            {
                if (fieldPtr < valuePtr || fieldPtr >= (byte*)valuePtr + sizeof(T))
                    throw new ArgumentException("invalid field");
                return (int)((byte*)fieldPtr - (byte*)valuePtr);
            }
        }

        protected override long BuildImpl(Stream stream, long dataPosition, long patchPosition)
        {
            foreach (var (offset, builder) in _builders)
            {
                // TODO: restrict on writing-size of field value?
                patchPosition = builder.Build(stream, dataPosition + offset, patchPosition);
            }
            return patchPosition;
        }
    }
}