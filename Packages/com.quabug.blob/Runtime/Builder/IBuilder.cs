using System.IO;
using JetBrains.Annotations;

namespace Blob
{
    public interface IBuilder
    {
        /// <summary>
        /// serialize builder value into BLOB stream
        /// </summary>
        /// <param name="stream">BLOB stream</param>
        /// <param name="dataPosition">begin position of current data</param>
        /// <param name="patchPosition">begin position of patched(dynamic) data</param>
        /// <returns>patch position after building</returns>
        long Build([NotNull] Stream stream, long dataPosition, long patchPosition);

        long Position { get; }
    }

    public interface IBuilder<T> : IBuilder where T : unmanaged {}

    public abstract class Builder<T> : IBuilder<T> where T : unmanaged
    {
        public long Position { get; private set; }

        public virtual long Build(Stream stream, long dataPosition, long patchPosition)
        {
            Position = dataPosition;
            stream.Seek(dataPosition, SeekOrigin.Begin);
            patchPosition = Utilities.EnsurePatchPosition<T>(patchPosition, dataPosition);
            return BuildImpl(stream, dataPosition, patchPosition);
        }

        protected abstract long BuildImpl([NotNull] Stream stream, long dataPosition, long patchPosition);
    }
}