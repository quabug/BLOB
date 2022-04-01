namespace Blob
{
    public interface IBlobStream
    {
        int PatchPosition { get; set; }
        int DataPosition { get; set; }
        int Length { get; set; }
        byte[] ToArray();
        unsafe void Write(byte* valuePtr, int size, int alignment);
    }
}