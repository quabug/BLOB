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

        public int DataPosition { get; }
        public int DataSize { get; }
        
        public int PatchPosition { get; }
        public int PatchSize { get; }
    }

    public interface IBuilder<T> : IBuilder where T : unmanaged {}

    public abstract class Builder<T> : IBuilder<T> where T : unmanaged
    {
        public int DataAlignment { get; set; } = 0;
        public int PatchAlignment { get; set; } = 0;
        
        public int DataPosition { get; private set; }
        public int DataSize { get; private set; }
        
        public int PatchPosition { get; private set; }
        public int PatchSize { get; private set; }

        public virtual unsafe void Build(IBlobStream stream)
        {
            DataPosition = stream.Position;
            DataSize = sizeof(T);
            ref var data = ref stream.EnsureDataAs<T>(stream.GetAlignment(DataAlignment));
            PatchPosition = stream.PatchPosition;
            BuildImpl(stream, ref data);
            if (PatchPosition != stream.PatchPosition) stream.AlignPatch(stream.GetAlignment(PatchAlignment));
            PatchSize = stream.PatchPosition - PatchPosition;
        }

        protected abstract void BuildImpl([NotNull] IBlobStream stream, ref T data);
    }
}