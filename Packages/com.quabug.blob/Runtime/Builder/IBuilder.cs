using JetBrains.Annotations;

namespace Blob
{
    public interface IBuilder
    {
        /// <summary>
        /// serialize builder value into BLOB stream
        /// </summary>
        /// <param name="stream">BLOB stream</param>
        /// <returns>patch position after building</returns>
        void Build([NotNull] IBlobStream stream);

        int Position { get; }
    }

    public interface IBuilder<T> : IBuilder where T : unmanaged {}

    public abstract class Builder<T> : IBuilder<T> where T : unmanaged
    {
        public int Position { get; private set; }

        public virtual void Build(IBlobStream stream)
        {
            Position = stream.DataPosition;
            BuildImpl(stream);
        }

        protected abstract void BuildImpl([NotNull] IBlobStream stream);
    }
}