﻿using System.Collections.Generic;
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

        public int Alignment { get; set; } = 0;

        public AnyTreeBuilder() {}

        public AnyTreeBuilder([NotNull] ITreeNode root)
        {
            Root = root;
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            var (endIndices, valueBuilders) = Flatten(Root);

            var dataBuilder = new AnyArrayBuilder(Alignment);
            foreach (var valueBuilder in valueBuilders) dataBuilder.Add(valueBuilder);

            var builder = new StructBuilder<BlobTreeAny>();
            builder.SetArray(ref builder.Value.EndIndices, endIndices);
            builder.SetBuilder(ref builder.Value.Data, dataBuilder);
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