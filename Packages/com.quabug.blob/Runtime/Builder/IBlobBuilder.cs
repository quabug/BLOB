using System.IO;

namespace Blob
{
    public interface IBlobBuilder
    {
        /// <summary>
        /// serialize builder value into BLOB stream
        /// </summary>
        /// <param name="stream">BLOB stream</param>
        /// <param name="dataPosition">begin position of current data</param>
        /// <param name="patchPosition">begin position of patched(dynamic) data</param>
        /// <returns>patch position after building</returns>
        long Build(Stream stream, long dataPosition, long patchPosition);
    }

    public interface IBlobBuilder<T> : IBlobBuilder where T : unmanaged {}
}