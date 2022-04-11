using System;

namespace Blob
{
    public class AnyValueBuilder : IBuilder
    {
        private byte[] _data;
        private int _alignment;

        public int Position { get; private set; }

        public void SetValue<T>(T value) where T : unmanaged
        {
            SetValue(value, Utilities.AlignOf<T>());
        }

        public void SetValue<T>(T value, int alignment) where T : unmanaged
        {
            if (!Utilities.IsPowerOfTwo(alignment)) throw new ArgumentException($"{nameof(alignment)} must be a power of two number");
            _data = ToBytes(value);
            _alignment = alignment > 0 ? alignment : Utilities.AlignOf<T>();
        }

        public void Build(IBlobStream stream)
        {
            Position = stream.DataPosition;
            stream.WriteArrayData(_data, _alignment);
        }

        private unsafe byte[] ToBytes<T>(T value) where T : unmanaged
        {
            var size = sizeof(T);
            if (size == 0) return Array.Empty<byte>();
            var bytes = new byte[size];
            fixed (void* destPtr = &bytes[0])
            {
                Buffer.MemoryCopy(&value, destPtr, size, size);
            }
            return bytes;
        }
    }
}