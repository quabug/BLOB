using System;
using JetBrains.Annotations;

namespace Blob
{
    public unsafe ref struct UnsafeBlobStreamValue<T> where T : unmanaged
    {
        private readonly IBlobStream _stream;
        private readonly int _position;

        public UnsafeBlobStreamValue([NotNull] IBlobStream stream) : this(stream, stream.DataPosition) {}
        public UnsafeBlobStreamValue([NotNull] IBlobStream stream, int position)
        {
            _stream = stream;
            _position = position;
            if (stream.Length <= position) throw new ArgumentException("invalid position");
        }

        public ref T Value
        {
            get
            {
                fixed (void* ptr = &_stream.Buffer[_position])
                {
                    return ref *(T*)ptr;
                }
            }
        }
    }
}
