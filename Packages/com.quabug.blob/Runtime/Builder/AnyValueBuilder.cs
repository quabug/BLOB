using System;

namespace Blob
{
    public class AnyValueBuilder : IBuilder
    {
        private byte[] _data;

        public int Alignment { get; set; } = 0;

        public int DataPosition { get; private set; }
        public int DataSize { get; private set; }
        public int PatchPosition { get; private set; }
        public int PatchSize { get; private set; }

        public void SetValue<T>(T value) where T : unmanaged
        {
            SetValue(value, Utilities.AlignOf<T>());
        }

        public void SetValue<T>(T value, int alignment) where T : unmanaged
        {
            if (!Utilities.IsPowerOfTwo(alignment)) throw new ArgumentException($"{nameof(alignment)} must be a power of two number");
            _data = ToBytes(value);
        }

        public void Build(IBlobStream stream)
        {
            DataPosition = stream.Position;
            DataSize = _data.Length;
            PatchPosition = stream.PatchPosition;
            stream.WriteArrayData(_data, stream.GetAlignment(Alignment));
            PatchSize = stream.PatchPosition - PatchPosition;
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