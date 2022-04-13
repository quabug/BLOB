using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobTree
    {
        class TreeNode<T> : ITreeNode<T> where T : unmanaged
        {
            internal readonly List<TreeNode<T>> InternalChildren = new List<TreeNode<T>>();
            private TreeNode<T> _parent;

            public IBuilder<T> ValueBuilder { get; }
            public int BlobIndex { get; set; } = -1;
            public IReadOnlyList<ITreeNode<T>> Children => InternalChildren;

            public TreeNode<T> Parent
            {
                get => _parent;
                set
                {
                    _parent?.InternalChildren.Remove(this);
                    _parent = value;
                    _parent.InternalChildren.Add(this);
                }
            }

            public TreeNode(T value) : this(value, Array.Empty<TreeNode<T>>()) {}
            public TreeNode(T value, IReadOnlyList<TreeNode<T>> children) : this(new ValueBuilder<T>(value), children) {}
            public TreeNode(IBuilder<T> valueBuilder) : this(valueBuilder, Array.Empty<TreeNode<T>>()) {}
            public TreeNode(IBuilder<T> valueBuilder, IReadOnlyList<TreeNode<T>> children)
            {
                ValueBuilder = valueBuilder;
                foreach (var child in children) child.Parent = this;
            }

            public int ParentIndex => Parent?.BlobIndex ?? -1;
            public IEnumerable<int> ChildrenIndices => InternalChildren.Select(child => child.BlobIndex);
            public IEnumerable<int> DescendantsIndices => ChildrenIndices.Concat(InternalChildren.SelectMany(child => child.DescendantsIndices));
            public IEnumerable<int> AncestorIndices
            {
                get
                {
                    var node = Parent;
                    while (node != null)
                    {
                        yield return node.BlobIndex;
                        node = node.Parent;
                    }
                }
            }
        }

        IReadOnlyList<TreeNode<int>> CreateRandomIntTree(int count, int seed)
        {
            var tree = Enumerable.Range(0, count).Select(value => new TreeNode<int>((value + 1) * 100)).ToArray();
            return RandomTree(tree, seed);
        }

        void CompareTree<T>(ref BlobTree<T> blobTree, IReadOnlyList<TreeNode<T>> tree) where T : unmanaged
        {
            Assert.That(blobTree.Length, Is.EqualTo(tree.Count));
            foreach (var node in tree)
            {
                var blobNode = blobTree[node.BlobIndex];
                CompareBlobNodeWithBuildNode(blobNode, node);
            }
        }

        void CompareBlobNodeWithBuildNode<T>(in BlobTree<T>.Node blobNode, TreeNode<T> buildNode) where T : unmanaged
        {
            // Assert.That(blobNode.ValueBuilder, Is.EqualTo(buildNode.ValueBuilder));
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
            var node = new TreeNode<int>(100);
            var builder = new TreeBuilder<int>(node);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(1));
            CompareBlobNodeWithBuildNode(blob.Value[0], node);
        }

        [Test]
        public void should_create_blob_tree_with_single_branch()
        {
            var nodes = CreateRandomIntTree(100, 0);
            for (var i = 1; i < nodes.Count; i++) nodes[i].Parent = nodes[i - 1];
            SetBlobIndex(nodes[0]);
            var builder = new TreeBuilder<int>(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_with_binary_branches()
        {
            var nodes = CreateRandomIntTree(100, 0);
            for (var i = 1; i < nodes.Count; i++) nodes[i].Parent = nodes[(i-1)/2];
            SetBlobIndex(nodes[0]);
            var builder = new TreeBuilder<int>(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_with_random_branches([Random(50)] int seed)
        {
            var nodes = CreateRandomIntTree(100, seed);
            var builder = new TreeBuilder<int>(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_array_of_blob_tree_with_random_branches([Random(10)] int seed)
        {
            var nodesList = Enumerable.Range(0, 5).Select(i => CreateRandomIntTree(100, seed + i)).ToArray();
            var treeBuilders = nodesList.Select(nodes => new TreeBuilder<int>(nodes[0])).ToArray();
            var builder = new ArrayBuilderWithItemBuilders<BlobTree<int>>(treeBuilders);
            var blob = builder.CreateManagedBlobAssetReference();

            for (var i = 0; i < nodesList.Length; i++) CompareTree(ref blob.Value[i], nodesList[i]);
        }

        [Test]
        public void should_create_ptr_of_blob_tree_with_random_branches([Random(50)] int seed)
        {
            var nodes = CreateRandomIntTree(100, seed);
            var treeBuilder = new TreeBuilder<int>(nodes[0]);
            var builder = new PtrBuilderWithNewValue<BlobTree<int>>(treeBuilder);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_of_ptr_with_random_branches([Random(50)] int seed)
        {
            var rawNodes = CreateRandomIntTree(100, seed);
            var nodes = rawNodes.Select(node => new TreeNode<BlobPtr<int>>(new PtrBuilderWithNewValue<int>(node.ValueBuilder))).ToArray();
            RandomTree(nodes, seed);
            var builder = new TreeBuilder<BlobPtr<int>>(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();

            for (var i = 0; i < nodes.Length; i++)
            {
                var index = blob.Value[i].Value.Value / 100 - 1;
                nodes[index].BlobIndex = i;
            }
            Assert.That(blob.Value.Length, Is.EqualTo(nodes.Length));
            for (var i = 0; i < nodes.Length; i++)
            {
                var blobNode = blob.Value[i];
                var node = nodes[blobNode.Value.Value / 100 - 1];
                var nodeValue = ((ValueBuilder<int>)((PtrBuilderWithNewValue<int>)node.ValueBuilder).ValueBuilder).Value;
                Assert.That(blobNode.Value.Value, Is.EqualTo(nodeValue));
                Assert.That(blobNode.FindParentIndex(), Is.EqualTo(node.ParentIndex));
                Assert.That(blobNode.FindAncestorsIndices(), Is.EquivalentTo(node.AncestorIndices));
                Assert.That(blobNode.FindDescendantsIndices(), Is.EquivalentTo(node.DescendantsIndices));
                Assert.That(blobNode.FindChildrenIndices(), Is.EquivalentTo(node.ChildrenIndices));
            }
        }

        IReadOnlyList<TreeNode<T>> RandomTree<T>(IReadOnlyList<TreeNode<T>> nodes) where T : unmanaged
        {
            return RandomTree(nodes, Environment.TickCount);
        }

        IReadOnlyList<TreeNode<T>> RandomTree<T>(IReadOnlyList<TreeNode<T>> nodes, int seed) where T : unmanaged
        {
            var random = new Random(seed);
            for (var i = 1; i < nodes.Count; i++)
            {
                var parentIndex = random.Next(i);
                nodes[i].Parent = nodes[parentIndex];
            }
            SetBlobIndex(nodes[0]);
            return nodes;
        }

        int SetBlobIndex<T>(TreeNode<T> root, int index = 0) where T : unmanaged
        {
            root.BlobIndex = index;
            foreach (var child in root.InternalChildren) index = SetBlobIndex(child, index + 1);
            return index;
        }

        struct ComplexBlob
        {
            public int A;
            public BlobTree<int> B;
            public BlobPtr<BlobTree<int>> C;
            public BlobArray<BlobTree<int>> D;
            public BlobString<UTF8Encoding> E;
            public BlobTree<BlobPtr<int>> F;
            public float G;
        }

        [Test]
        public void should_create_complex_blob_with_blob_tree([Random(10)] int seed)
        {
            var builder = new StructBuilder<ComplexBlob>();
            builder.SetValue(ref builder.Value.A, 123);
            var nodes = CreateRandomIntTree(100, seed);
            var treeBuilder = builder.SetTree(ref builder.Value.B, nodes[0]);
            builder.SetPointer(ref builder.Value.C, treeBuilder);
            var nodes1 = CreateRandomIntTree(10, seed);
            var nodes2 = CreateRandomIntTree(50, seed);
            var nodes3 = CreateRandomIntTree(77, seed);
            var tree1 = new TreeBuilder<int>(nodes1[0]);
            var tree2 = new TreeBuilder<int>(nodes2[0]);
            var tree3 = new TreeBuilder<int>(nodes3[0]);
            builder.SetArray(ref builder.Value.D, new[] { tree1, tree2, tree3 });
            builder.SetString(ref builder.Value.E, "fdajkrewupfnk");
            var ptrNodes = nodes.Select(node => new TreeNode<BlobPtr<int>>(new PtrBuilderWithNewValue<int>(node.ValueBuilder))).ToArray();
            RandomTree(ptrNodes, seed);
            builder.SetTree(ref builder.Value.F, ptrNodes[0]);
            builder.SetValue(ref builder.Value.G, 222.1f);

            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.A, Is.EqualTo(123));
            CompareTree(ref blob.Value.B, nodes);
            CompareTree(ref blob.Value.C.Value, nodes);
            CompareTree(ref blob.Value.D[0], nodes1);
            CompareTree(ref blob.Value.D[1], nodes2);
            CompareTree(ref blob.Value.D[2], nodes3);
            Assert.That(blob.Value.E.ToString(), Is.EqualTo("fdajkrewupfnk"));

            for (var i = 0; i < ptrNodes.Length; i++)
            {
                var index = blob.Value.F[i].Value.Value / 100 - 1;
                ptrNodes[index].BlobIndex = i;
            }
            Assert.That(blob.Value.F.Length, Is.EqualTo(ptrNodes.Length));
            for (var i = 0; i < ptrNodes.Length; i++)
            {
                var blobNode = blob.Value.F[i];
                var node = ptrNodes[blobNode.Value.Value / 100 - 1];
                var nodeValue = ((ValueBuilder<int>)((PtrBuilderWithNewValue<int>)node.ValueBuilder).ValueBuilder).Value;
                Assert.That(blobNode.Value.Value, Is.EqualTo(nodeValue));
                Assert.That(blobNode.FindParentIndex(), Is.EqualTo(node.ParentIndex));
                Assert.That(blobNode.FindAncestorsIndices(), Is.EquivalentTo(node.AncestorIndices));
                Assert.That(blobNode.FindDescendantsIndices(), Is.EquivalentTo(node.DescendantsIndices));
                Assert.That(blobNode.FindChildrenIndices(), Is.EquivalentTo(node.ChildrenIndices));
            }

            Assert.That(blob.Value.G, Is.EqualTo(222.1f));
        }
    }
}