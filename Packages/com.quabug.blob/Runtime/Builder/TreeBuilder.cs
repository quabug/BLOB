using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob
{
    public interface ITreeNode<out T>
    {
        T Value { get; }
        IReadOnlyList<ITreeNode<T>> Children { get; }
    }

    public class TreeBuilder<T> : Builder<BlobTree<T>> where T : unmanaged
    {
        private readonly StructBuilder<BlobTree<T>> _builder = new StructBuilder<BlobTree<T>>();

        public TreeBuilder()
        {
            _builder.SetArray(ref _builder.Value.EndIndices, Array.Empty<int>());
            _builder.SetArray(ref _builder.Value.Nodes, Array.Empty<T>());
        }

        public TreeBuilder([NotNull] ITreeNode<T> root) : this(root, Utilities.AlignOf<T>()) {}

        public TreeBuilder([NotNull] ITreeNode<T> root, int alignment)
        {
            var (endIndices, values) = Flatten(root);
            _builder.SetArray(ref _builder.Value.EndIndices, endIndices);
            _builder.SetArray(ref _builder.Value.Nodes, values, alignment);
        }

        public TreeBuilder([NotNull] ITreeNode<IBuilder<T>> root)
        {
            var (endIndices, values) = Flatten(root);
            _builder.SetArray(ref _builder.Value.EndIndices, endIndices);
            _builder.SetArray(ref _builder.Value.Nodes, values);
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            _builder.Build(stream);
        }

        private (List<int> endIndices, List<U> nodeValues) Flatten<U>(ITreeNode<U> root)
        {
            var endIndices = new List<int>();
            var values = new List<U>();
            FlattenAndReturnEndIndex(root, 0);
            return (endIndices, values);

            int /*endIndex*/ FlattenAndReturnEndIndex(ITreeNode<U> node, int index)
            {
                var valueIndex = values.Count;
                values.Add(node.Value);
                endIndices.Add(-1);
                var endIndex = index + 1;
                foreach (var child in node.Children) endIndex = FlattenAndReturnEndIndex(child, endIndex);
                endIndices[valueIndex] = endIndex;
                return endIndex;
            }
        }
    }
}