using System;
using System.IO;

namespace Blob
{
    public class BlobMemoryStream : IBlobStream, IDisposable
    {
        private readonly MemoryStream _stream = new MemoryStream(1024 * 8);

        public int PatchPosition { get; set; }

        public int DataPosition
        {
            get => (int)_stream.Position;
            set => _stream.Position = value;
        }

        public int Length
        {
            get => (int)_stream.Length;
            set => _stream.SetLength(value);
        }

        public byte[] ToArray() => _stream.ToArray();

        public unsafe void Write(byte* valuePtr, int size, int alignment)
        {
            PatchPosition = Math.Max(PatchPosition, (int)Utilities.Align(DataPosition + size, alignment));
#if UNITY_2021_2_OR_NEWER || NETSTANDARD2_1_OR_GREATER
            _stream.Write(new System.ReadOnlySpan<byte>(valuePtr, size));
#else
            // `WriteByte` is the fastest way to write binary into stream on small chunks (<8192bytes) based on my benchmark
            // and BLOB intend to use on small chunks(? or maybe not?)
            // so I decide just use this simple way here
            for (var i = 0; i < size; i++) _stream.WriteByte(*(valuePtr + i));
#endif
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}