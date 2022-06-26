using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public interface ITreeNode
    {
        IBuilder ValueBuilder { get; }
        IReadOnlyList<ITreeNode> Children { get; }
    }

    public class AnyTreeBuilder : Builder<BlobTreeAny>
    {
        public ITreeNode Root { get; set; }

        public int Alignment
        {
            get => ArrayBuilder.Alignment;
            set => ArrayBuilder.Alignment = value;
        }

        public AnyArrayBuilder ArrayBuilder { get; } = new AnyArrayBuilder();

        public AnyTreeBuilder() {}

        public AnyTreeBuilder([NotNull] ITreeNode root)
        {
            Root = root;
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            var (endIndices, valueBuilders) = Flatten(Root);

            foreach (var valueBuilder in valueBuilders) ArrayBuilder.Add(valueBuilder);

            var builder = new StructBuilder<BlobTreeAny>();
            builder.SetArray(ref builder.Value.EndIndices, endIndices);
            builder.SetBuilder(ref builder.Value.Data, ArrayBuilder);
            builder.Build(stream);
        }

        private (List<int> endIndices, List<IBuilder> builders) Flatten(ITreeNode root)
        {
            var endIndices = new List<int>();
            var values = new List<IBuilder>();
            FlattenAndReturnEndIndex(root, 0);
            return (endIndices, values);

            int /*endIndex*/ FlattenAndReturnEndIndex(ITreeNode node, int index)
            {
                if (node == null) return index;
                var valueIndex = values.Count;
                values.Add(node.ValueBuilder);
                endIndices.Add(-1);
                var endIndex = index + 1;
                foreach (var child in node.Children) endIndex = FlattenAndReturnEndIndex(child, endIndex);
                endIndices[valueIndex] = endIndex;
                return endIndex;
            }
        }
    }
}