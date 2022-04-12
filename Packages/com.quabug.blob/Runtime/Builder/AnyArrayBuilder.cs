using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public class AnyArrayBuilder : Builder<BlobArrayAny>
    {
        private readonly List<IBuilder> _builderList = new List<IBuilder>();
        private readonly int _alignment = 0;

        public AnyArrayBuilder() {}
        public AnyArrayBuilder(int alignment)
        {
            if (!Utilities.IsPowerOfTwo(alignment)) throw new ArgumentException($"{nameof(alignment)} must be power of 2");
            _alignment = alignment;
        }

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

        protected override void BuildImpl(IBlobStream stream)
        {
            // write meta of Offsets:BlobArray<int>
            var offsetLength = _builderList.Count + 1;
            stream.EnsureDataSize<BlobArrayAny>().WriteArrayMeta(offsetLength);

            var dataArrayPosition = stream.DataPosition;
            var offsetPatchPosition = stream.PatchPosition;
            // TODO: stackalloc for frameworks later than .NET Standard 2.1?
            var offsets = new int[offsetLength];

            // write data of Data:BlobArray<byte>
            // and fill offsets
            stream.ExpandPatch(sizeof(int) * offsetLength, Utilities.AlignOf<int>()).ToPatchPosition();
            var dataPatchPosition = stream.DataPosition;
            var position = stream.DataPosition;
            for (var i = 0; i < _builderList.Count; i++)
            {
                offsets[i] = stream.DataPosition - position;
                _builderList[i].Build(stream);
            }
            var dataLength = stream.DataPosition - position;
            offsets[_builderList.Count] = dataLength;

            // write meta of Data:BlobArray<byte>
            stream.ToPosition(dataArrayPosition).WriteArrayMeta(dataLength, dataPatchPosition - dataArrayPosition);

            // write data of Offsets:BlobArray<int>
            stream.ToPosition(offsetPatchPosition).WriteArrayData(offsets);
        }

        private int GetAlignment<T>() where T : unmanaged
        {
            return _alignment > 0 ? _alignment : Utilities.AlignOf<T>();
        }
    }
}