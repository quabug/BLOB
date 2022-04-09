using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobTree
    {
        class TreeNode : ITreeNode<int>
        {
            private readonly List<TreeNode> _children = new List<TreeNode>();
            private TreeNode _parent;

            public int Value { get; }
            public int Index { get; set; }
            public IReadOnlyList<ITreeNode<int>> Children => _children;

            public TreeNode Parent
            {
                get => _parent;
                set
                {
                    _parent?._children.Remove(this);
                    _parent = value;
                    _parent._children.Add(this);
                }
            }

            public TreeNode(int value) : this(value, Array.Empty<TreeNode>()) {}
            public TreeNode(int value, IReadOnlyList<TreeNode> children)
            {
                Value = value;
                foreach (var child in children) child.Parent = this;
            }

            public int ParentIndex => Parent?.Index ?? -1;
            public IEnumerable<int> ChildrenIndices => _children.Select(child => child.Index);
            public IEnumerable<int> DescendantsIndices => ChildrenIndices.Concat(_children.SelectMany(child => child.DescendantsIndices));
            public IEnumerable<int> AncestorIndices
            {
                get
                {
                    var node = Parent;
                    while (node != null)
                    {
                        yield return node.Index;
                        node = node.Parent;
                    }
                }
            }
        }

        private TreeNode[] _nodes;

        [SetUp]
        public void SetUp()
        {
            _nodes = Enumerable.Range(0, 100).Select(value => new TreeNode((value + 1) * 100)).ToArray();
        }

        void CompareTree(ManagedBlobAssetReference<BlobTree<int>> blobTree, TreeNode[] tree)
        {
            SetNodesIndex();
            Assert.That(blobTree.Value.Length, Is.EqualTo(_nodes.Length));
            for (var i = 0; i < _nodes.Length; i++)
            {
                var blobNode = blobTree.Value[i];
                var node = _nodes[blobNode.Value / 100 - 1];
                CompareBlobNodeWithBuildNode(blobNode, node);
            }

            void SetNodesIndex()
            {
                for (var i = 0; i < tree.Length; i++)
                {
                    var index = blobTree.Value[i].Value / 100 - 1;
                    tree[index].Index = i;
                }
            }
        }

        void CompareBlobNodeWithBuildNode(in BlobTree<int>.Node blobNode, TreeNode buildNode)
        {
            Assert.That(blobNode.Value, Is.EqualTo(buildNode.Value));
            Assert.That(blobNode.FindParentIndex(), Is.EqualTo(buildNode.ParentIndex));
            Assert.That(blobNode.FindAncestorsIndices(), Is.EquivalentTo(buildNode.AncestorIndices));
            Assert.That(blobNode.FindDescendantsIndices(), Is.EquivalentTo(buildNode.DescendantsIndices));
            Assert.That(blobNode.FindChildrenIndices(), Is.EquivalentTo(buildNode.ChildrenIndices));
        }

        [Test]
        public void should_create_blob_tree_with_empty_node()
        {
            var builder = new TreeBuilder<int>();
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(0));
        }

        [Test]
        public void should_create_blob_tree_with_single_node()
        {
            var builder = new TreeBuilder<int>(_nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(1));
            CompareBlobNodeWithBuildNode(blob.Value[0], _nodes[0]);
        }

        [Test]
        public void should_create_blob_tree_with_single_branch()
        {
            for (var i = 1; i < _nodes.Length; i++) _nodes[i].Parent = _nodes[i - 1];
            var builder = new TreeBuilder<int>(_nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(blob, _nodes);
        }

        [Test]
        public void should_create_blob_tree_with_binary_branches()
        {
            for (var i = 1; i < _nodes.Length; i++) _nodes[i].Parent = _nodes[(i-1)/2];
            var builder = new TreeBuilder<int>(_nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(blob, _nodes);
        }

        [Test]
        public void should_create_blob_tree_with_random_branches([Random(50)] int seed)
        {
            var random = new Random(seed);
            for (var i = 1; i < _nodes.Length; i++)
            {
                var parentIndex = random.Next(i);
                _nodes[i].Parent = _nodes[parentIndex];
            }
            var builder = new TreeBuilder<int>(_nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(blob, _nodes);
        }
    }
}