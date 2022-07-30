using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public class AnyArrayBuilder : Builder<BlobArrayAny>
    {
        private readonly List<IBuilder> _builderList = new List<IBuilder>();
        public int Alignment { get; set; } = 0;

        public int Count => _builderList.Count;

        public void Insert<T>(int index, T item) where T : unmanaged
        {
            var builder = new AnyValueBuilder();
            builder.SetValue(item, GetAlignment<T>());
            _builderList.Insert(index, builder);
        }

        public void Add<T>(T item) where T : unmanaged
        {
            var builder = new AnyValueBuilder();
            builder.SetValue(item, GetAlignment<T>());
            _builderList.Add(builder);
        }

        public void Add([NotNull] IBuilder itemBuilder)
        {
            _builderList.Add(itemBuilder);
        }

        public void RemoveAt(int index)
        {
            _builderList.RemoveAt(index);
        }

        public void Clear()
        {
            _builderList.Clear();
        }

        protected override unsafe void BuildImpl(IBlobStream stream, ref BlobArrayAny data)
        {
            // write meta of Offsets:BlobArray<int>
            var offsetLength = _builderList.Count + 1;
            data.Offsets.Length = offsetLength;
            data.Offsets.Offset = stream.PatchOffset() - data.GetFieldOffset(ref data.Offsets.Offset);

            // TODO: stackalloc for frameworks later than .NET Standard 2.1?
            var offsets = new int[offsetLength];

            // reserve space of offset array
            stream.ExpandPatch(sizeof(int) * offsetLength, stream.GetAlignment(PatchAlignment));
            data.Data.Offset = stream.PatchOffset() - data.GetFieldOffset(ref data.Data.Offset);
            
            // write data of Data:BlobArray<byte>
            // and fill offsets
            var position = stream.PatchPosition;
            for (var i = 0; i < _builderList.Count; i++)
            {
                offsets[i] = stream.PatchPosition - position;
                stream.ToPatchPosition();
                _builderList[i].Build(stream);
            }
            var patchSize = stream.PatchPosition - position;
            offsets[_builderList.Count] = patchSize;

            data.Data.Length = patchSize;

            // write data of Offsets:BlobArray<int>
            stream.ToPosition(PatchPosition).WriteArrayData(offsets);
        }

        private int GetAlignment<T>() where T : unmanaged
        {
            return Alignment > 0 ? Alignment : Utilities.AlignOf<T>();
        }
    }
}