using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public interface ITreeNode<T> where T : unmanaged
    {
        IBuilder<T> ValueBuilder { get; }
        IReadOnlyList<ITreeNode<T>> Children { get; }
    }

    public class TreeBuilder<T> : Builder<BlobTree<T>> where T : unmanaged
    {
        private readonly StructBuilder<BlobTree<T>> _builder = new StructBuilder<BlobTree<T>>();

        public int Alignment { get; set; } = Utilities.AlignOf<T>();

        public TreeBuilder()
        {
            _builder.SetArray(ref _builder.Value.EndIndices, Array.Empty<int>());
            _builder.SetArray(ref _builder.Value.Nodes, Array.Empty<T>());
        }

        public TreeBuilder([NotNull] ITreeNode<T> root)
        {
            var (endIndices, valueBuilders) = Flatten(root);
            _builder.SetArray(ref _builder.Value.EndIndices, endIndices);
            _builder.SetArray(ref _builder.Value.Nodes, valueBuilders, Alignment);
        }

        protected override void BuildImpl(IBlobStream stream, ref BlobTree<T> data)
        {
            _builder.Build(stream);
        }

        private (List<int> endIndices, List<IBuilder<T>> valueBuilders) Flatten(ITreeNode<T> root)
        {
            var endIndices = new List<int>();
            var valueBuilders = new List<IBuilder<T>>();
            FlattenAndReturnEndIndex(root, 0);
            return (endIndices, valueBuilders);

            int /*endIndex*/ FlattenAndReturnEndIndex(ITreeNode<T> node, int index)
            {
                var valueIndex = valueBuilders.Count;
                valueBuilders.Add(node.ValueBuilder);
                endIndices.Add(-1);
                var endIndex = index + 1;
                foreach (var child in node.Children) endIndex = FlattenAndReturnEndIndex(child, endIndex);
                endIndices[valueIndex] = endIndex;
                return endIndex;
            }
        }
    }
}