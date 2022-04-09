using System.Collections.Generic;

namespace Blob.Tests
{
    public struct BlobTree<T> where T : unmanaged
    {
        public BlobArray<int> EndIndices;
        public BlobArray<T> Nodes;

        public Node this[int index] => new Node(ref this, index);
        public int Length => Nodes.Length;

        public readonly unsafe ref struct Node
        {
            private readonly BlobTree<T>* _treePtr;
            private readonly int _index;

            public ref BlobTree<T> Tree => ref *_treePtr;
            public ref T Value => ref Tree.Nodes[_index];
            public int Index => _index;
            public int EndIndex => Tree.EndIndices[_index];

            internal Node(ref BlobTree<T> tree, int index)
            {
                fixed (BlobTree<T>* ptr = &tree) _treePtr = ptr;
                _index = index;
            }

            public int FindParentIndex() => Tree.FindParentIndex(Index);

            public List<int> FindAncestorsIndices() => Tree.FindAncestorsIndices(Index);
            public List<int> FindChildrenIndices() => Tree.FindChildrenIndices(Index);
            public List<int> FindDescendantsIndices() => Tree.FindDescendantsIndices(Index);

            public void FindAncestorsIndices(ICollection<int> output) => Tree.FindAncestorsIndices(Index, output);
            public void FindChildrenIndices(ICollection<int> output) => Tree.FindChildrenIndices(Index, output);
            public void FindDescendantsIndices(ICollection<int> output) => Tree.FindDescendantsIndices(Index, output);

            public void ForEachAncestors(NodeAction action) => Tree.ForEachAncestors(Index, action);
            public void ForEachChildren(NodeAction action) => Tree.ForEachChildren(Index, action);
            public void ForEachDescendants(NodeAction action) => Tree.ForEachDescendants(Index, action);
        }

        public int FindParentIndex(int index)
        {
            var endIndex = EndIndices[index];
            for (var i = index - 1; i >= 0; i--)
            {
                if (EndIndices[i] >= endIndex) return i;
            }
            return -1;
        }

        public List<int> FindAncestorsIndices(int index)
        {
            var indices = new List<int>();
            FindAncestorsIndices(index, indices);
            return indices;
        }

        public List<int> FindChildrenIndices(int index)
        {
            var indices = new List<int>();
            FindChildrenIndices(index, indices);
            return indices;
        }

        public List<int> FindDescendantsIndices(int index)
        {
            var indices = new List<int>();
            FindDescendantsIndices(index, indices);
            return indices;
        }

        public void FindAncestorsIndices(int index, ICollection<int> output)
        {
            ForEachAncestors(index, (in Node node) => output.Add(node.Index));
        }

        public void FindChildrenIndices(int index, ICollection<int> output)
        {
            ForEachChildren(index, (in Node node) => output.Add(node.Index));
        }

        public void FindDescendantsIndices(int index, ICollection<int> output)
        {
            ForEachDescendants(index, (in Node node) => output.Add(node.Index));
        }

        public delegate void NodeAction(in Node node);

        public void ForEachAncestors(int index, NodeAction action)
        {
            var endIndex = EndIndices[index];
            for (var i = index - 1; i >= 0; i--)
            {
                if (EndIndices[i] >= endIndex)
                {
                    action(this[i]);
                    endIndex = EndIndices[i];
                }
            }
        }

        public void ForEachChildren(int index, NodeAction action)
        {
            var endIndex = EndIndices[index];
            var childIndex = index + 1;
            while (childIndex < endIndex)
            {
                action(this[childIndex]);
                childIndex = EndIndices[childIndex];
            }
        }

        public void ForEachDescendants(int index, NodeAction action)
        {
            var endIndex = EndIndices[index];
            var startIndex = index + 1;
            for (var i = startIndex; i < endIndex; i++) action(this[i]);
        }
    }
}