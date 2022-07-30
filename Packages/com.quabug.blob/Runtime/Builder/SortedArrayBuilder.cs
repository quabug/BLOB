using System.Collections.Generic;
using System.Linq;

namespace Blob
{
    public class SortedArrayBuilder<TKey, TValue> : Builder<BlobSortedArray<TKey, TValue>>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        public IReadOnlyDictionary<TKey, IBuilder<TValue>> Builders { get; }
        public IBuilder<TValue> this[TKey key] => Builders[key];

        public SortedArrayBuilder() : this(Enumerable.Empty<(TKey, TValue)>()) {}

        public SortedArrayBuilder(IEnumerable<(TKey key, TValue value)> items)
            : this(items.Select(t => (t.key, (IBuilder<TValue>)new ValueBuilder<TValue>(t.value))))
        {
        }

        public SortedArrayBuilder(IEnumerable<(TKey key, IBuilder<TValue> valueBuilder)> items)
        {
            Builders = items.ToDictionary(t => t.key, t => t.valueBuilder);
        }

        public SortedArrayBuilder(IReadOnlyDictionary<TKey, TValue> items)
            : this(items.Select(t => (t.Key,t.Value)))
        {
        }

        public SortedArrayBuilder(IReadOnlyDictionary<TKey, IBuilder<TValue>> items)
            : this(items.Select(t => (t.Key, t.Value)))
        {
        }

        protected override void BuildImpl(IBlobStream stream, ref BlobSortedArray<TKey, TValue> data)
        {
            var builder = new StructBuilder<BlobSortedArray<TKey, TValue>> { DataAlignment = DataAlignment, PatchAlignment = PatchAlignment };
            var nodes = Builders.OrderBy(node => node.Key.GetHashCode()).ToArray();
            builder.SetArray(ref builder.Value.Keys, nodes.Select(node => node.Key));
            builder.SetArray(ref builder.Value.Values, nodes.Select(node => node.Value));
            builder.Build(stream);
        }
    }
}