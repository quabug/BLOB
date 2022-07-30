using JetBrains.Annotations;

namespace Blob
{
    public interface IBlobStream
    {
        int Alignment { get; set; }
        int PatchPosition { get; set; }
        int Position { get; set; }
        int Length { get; set; }
        byte[] ToArray();
        byte[] Buffer { get; }
        unsafe void Write(byte* valuePtr, int size, int alignment);
    }

    public static partial class BlobStreamExtension
    {
        public static unsafe void Write([NotNull] this IBlobStream stream, byte* valuePtr, int size) => stream.Write(valuePtr, size, stream.Alignment);
        public static int GetAlignment([NotNull] this IBlobStream stream, int alignment) => alignment > 0 ? alignment : stream.Alignment;
    }
}