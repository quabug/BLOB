using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobTreeAny
    {
        class TreeNode : ITreeNode
        {
            internal readonly List<TreeNode> InternalChildren = new List<TreeNode>();
            private TreeNode _parent;

            public int BlobIndex { get; set; }
            public IBuilder ValueBuilder { get; set; }

            IReadOnlyList<ITreeNode> ITreeNode.Children => InternalChildren;

            public TreeNode Parent
            {
                get => _parent;
                set
                {
                    _parent?.InternalChildren.Remove(this);
                    _parent = value;
                    _parent.InternalChildren.Add(this);
                }
            }

            public TreeNode(IBuilder valueBuilder) => ValueBuilder = valueBuilder;

            public void SetValue<T>(T value) where T : unmanaged
            {
                ValueBuilder = new ValueBuilder<T>(value);
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

        IReadOnlyList<TreeNode> CreateRandomIntTree(int count, int seed)
        {
            var tree = Enumerable.Range(0, count).Select(value => new TreeNode(new ValueBuilder<int>((value + 1) * 100))).ToArray();
            return RandomTree(tree, seed);
        }

        void CompareTree(ref BlobTreeAny blobTree, IReadOnlyList<TreeNode> tree)
        {
            Assert.That(blobTree.Length, Is.EqualTo(tree.Count));
            foreach (var node in tree)
            {
                var blobNode = blobTree[node.BlobIndex];
                CompareBlobNodeWithBuildNode(blobNode, node);
            }
        }

        void CompareBlobNodeWithBuildNode(in BlobTreeAny.Node blobNode, TreeNode buildNode)
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
            var builder = new AnyTreeBuilder();
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(0));
        }

        [Test]
        public void should_create_blob_tree_with_single_node()
        {
            var node = new TreeNode(new ValueBuilder<int>(100));
            var builder = new AnyTreeBuilder(node);
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
            var builder = new AnyTreeBuilder(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_with_binary_branches()
        {
            var nodes = CreateRandomIntTree(100, 0);
            for (var i = 1; i < nodes.Count; i++) nodes[i].Parent = nodes[(i-1)/2];
            SetBlobIndex(nodes[0]);
            var builder = new AnyTreeBuilder(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_with_random_branches([Random(50)] int seed)
        {
            var nodes = CreateRandomIntTree(100, seed);
            var builder = new AnyTreeBuilder(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value, nodes);
        }

        [Test]
        public void should_create_array_of_blob_tree_with_random_branches([Random(10)] int seed)
        {
            var nodesList = Enumerable.Range(0, 5).Select(i => CreateRandomIntTree(100, seed + i)).ToArray();
            var treeBuilders = nodesList.Select(nodes => new AnyTreeBuilder(nodes[0])).ToArray();
            var builder = new ArrayBuilderWithItemBuilders<BlobTreeAny>(treeBuilders);
            var blob = builder.CreateManagedBlobAssetReference();

            for (var i = 0; i < nodesList.Length; i++) CompareTree(ref blob.Value[i], nodesList[i]);
        }

        [Test]
        public void should_create_ptr_of_blob_tree_with_random_branches([Random(50)] int seed)
        {
            var nodes = CreateRandomIntTree(100, seed);
            var treeBuilder = new AnyTreeBuilder(nodes[0]);
            var builder = new PtrBuilderWithNewValue<BlobTreeAny>(treeBuilder);
            var blob = builder.CreateManagedBlobAssetReference();
            CompareTree(ref blob.Value.Value, nodes);
        }

        [Test]
        public void should_create_blob_tree_of_ptr_with_random_branches([Random(50)] int seed)
        {
            var rawNodes = CreateRandomIntTree(100, seed);
            var nodes = rawNodes.Select((node, i) => new TreeNode(new PtrBuilderWithNewValue<int>((i + 1) * 100))).ToArray();
            RandomTree(nodes, seed);
            var builder = new AnyTreeBuilder(nodes[0]);
            var blob = builder.CreateManagedBlobAssetReference();

            Assert.That(blob.Value.Length, Is.EqualTo(nodes.Length));
            foreach (var node in nodes)
            {
                var blobNode = blob.Value[node.BlobIndex];
                var nodeValue = ((ValueBuilder<int>)((PtrBuilderWithNewValue<int>)node.ValueBuilder).ValueBuilder).Value;
                Assert.That(blobNode.GetValue<BlobPtr<int>>().Value, Is.EqualTo(nodeValue));
                Assert.That(blobNode.FindParentIndex(), Is.EqualTo(node.ParentIndex));
                Assert.That(blobNode.FindAncestorsIndices(), Is.EquivalentTo(node.AncestorIndices));
                Assert.That(blobNode.FindDescendantsIndices(), Is.EquivalentTo(node.DescendantsIndices));
                Assert.That(blobNode.FindChildrenIndices(), Is.EquivalentTo(node.ChildrenIndices));
            }
        }

        IReadOnlyList<TreeNode> RandomTree(IReadOnlyList<TreeNode> nodes)
        {
            return RandomTree(nodes, Environment.TickCount);
        }

        IReadOnlyList<TreeNode> RandomTree(IReadOnlyList<TreeNode> nodes, int seed)
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

        int SetBlobIndex(TreeNode root, int index = 0)
        {
            root.BlobIndex = index;
            foreach (var child in root.InternalChildren) index = SetBlobIndex(child, index + 1);
            return index;
        }

        struct ComplexBlob
        {
            public int A;
            public BlobTreeAny B;
            public BlobPtr<BlobTreeAny> C;
            public BlobArray<BlobTreeAny> D;
            public BlobString<UTF8Encoding> E;
            public BlobTreeAny/*<BlobPtr<int>>*/ F;
            public float G;
        }

        [Test]
        public void should_create_complex_blob_with_blob_tree([Random(10)] int seed)
        {
            var builder = new StructBuilder<ComplexBlob>();
            builder.SetValue(ref builder.Value.A, 123);
            var nodes = CreateRandomIntTree(100, seed);
            var treeBuilder = builder.SetTreeAny(ref builder.Value.B, nodes[0]);
            builder.SetPointer(ref builder.Value.C, treeBuilder);
            var nodes1 = CreateRandomIntTree(10, seed);
            var nodes2 = CreateRandomIntTree(50, seed);
            var nodes3 = CreateRandomIntTree(77, seed);
            var tree1 = new AnyTreeBuilder(nodes1[0]);
            var tree2 = new AnyTreeBuilder(nodes2[0]);
            var tree3 = new AnyTreeBuilder(nodes3[0]);
            builder.SetArray(ref builder.Value.D, new[] { tree1, tree2, tree3 });
            builder.SetString(ref builder.Value.E, "fdajkrewupfnk");
            var ptrNodes = nodes.Select((node, i) => new TreeNode(new PtrBuilderWithNewValue<int>((i + 1) * 100))).ToArray();
            RandomTree(ptrNodes, seed);
            builder.SetTreeAny(ref builder.Value.F, ptrNodes[0]);
            builder.SetValue(ref builder.Value.G, 222.1f);

            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.A, Is.EqualTo(123));
            CompareTree(ref blob.Value.B, nodes);
            CompareTree(ref blob.Value.C.Value, nodes);
            CompareTree(ref blob.Value.D[0], nodes1);
            CompareTree(ref blob.Value.D[1], nodes2);
            CompareTree(ref blob.Value.D[2], nodes3);
            Assert.That(blob.Value.E.ToString(), Is.EqualTo("fdajkrewupfnk"));

            Assert.That(blob.Value.F.Length, Is.EqualTo(ptrNodes.Length));
            foreach (var node in ptrNodes)
            {
                var blobNode = blob.Value.F[node.BlobIndex];
                var nodeValue = ((ValueBuilder<int>)((PtrBuilderWithNewValue<int>)node.ValueBuilder).ValueBuilder).Value;
                Assert.That(blobNode.GetValue<BlobPtr<int>>().Value, Is.EqualTo(nodeValue));
                Assert.That(blobNode.FindParentIndex(), Is.EqualTo(node.ParentIndex));
                Assert.That(blobNode.FindAncestorsIndices(), Is.EquivalentTo(node.AncestorIndices));
                Assert.That(blobNode.FindDescendantsIndices(), Is.EquivalentTo(node.DescendantsIndices));
                Assert.That(blobNode.FindChildrenIndices(), Is.EquivalentTo(node.ChildrenIndices));
            }

            Assert.That(blob.Value.G, Is.EqualTo(222.1f));
        }

        [Test]
        public void should_create_blob_tree_with_different_types()
        {
            var intNode = new TreeNode(new ValueBuilder<int>(100));
            var longNode = new TreeNode(new ValueBuilder<long>(200));
            var doubleNode = new TreeNode(new ValueBuilder<double>(300));
            var floatArrayNode = new TreeNode(new ArrayBuilder<float>(new[] { 1f, 2f, 3f }));
            var shortPtrNode = new TreeNode(new PtrBuilderWithNewValue<short>(3));
            longNode.Parent = intNode;
            doubleNode.Parent = intNode;
            floatArrayNode.Parent = longNode;
            shortPtrNode.Parent = floatArrayNode;
            var builder = new AnyTreeBuilder(intNode);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(5));
            Assert.That(blob.Value[0].GetValue<int>(), Is.EqualTo(100));
            Assert.That(blob.Value[1].GetValue<long>(), Is.EqualTo(200));
            Assert.That(blob.Value[2].GetValue<BlobArray<float>>().ToArray(), Is.EqualTo(new [] { 1f, 2f, 3f }));
            Assert.That(blob.Value[3].GetValue<BlobPtr<short>>().Value, Is.EqualTo(3));
            Assert.That(blob.Value[4].GetValue<double>(), Is.EqualTo(300));
        }
    }
}