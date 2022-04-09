using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blob.Tests
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
            var endIndices = new List<int>();
            var values = new List<T>();

            FlattenAndReturnEndIndex(root, 0);

            _builder.SetArray(ref _builder.Value.EndIndices, endIndices);
            _builder.SetArray(ref _builder.Value.Nodes, values, alignment);

            int /*endIndex*/ FlattenAndReturnEndIndex(ITreeNode<T> node, int index)
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

        protected override void BuildImpl(IBlobStream stream)
        {
            _builder.Build(stream);
        }
    }
}