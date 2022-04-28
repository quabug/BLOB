using System;
using System.Collections.Generic;

namespace Blob
{
    public struct BlobTreeAny
    {
        public BlobArray<int> EndIndices;
        public BlobArrayAny Data;

        public Node this[int index] => new Node(ref this, index);
        public int Length => EndIndices.Length;

        public unsafe void* UnsafeDataPtr => Data.GetUnsafeValuePtr(0);
        public int DataSize => Data.Data.Length;

        public readonly unsafe ref struct Node
        {
            private readonly BlobTreeAny* _treePtr;
            private readonly int _index;

            public ref BlobTreeAny Tree => ref *_treePtr;
            public int Size => Tree.Data.GetSize(_index);
            public void* UnsafePtr => Tree.Data.GetUnsafeValuePtr(_index);

            public T* GetValuePtr<T>() where T : unmanaged
            {
                if (Size < sizeof(T)) throw new ArgumentException("invalid generic parameter");
                return (T*)UnsafePtr;
            }

            public ref T GetValue<T>() where T : unmanaged => ref *GetValuePtr<T>();
            public int Index => _index;
            public int EndIndex => Tree.EndIndices[_index];
            public int Offset => Tree.Data.GetOffset(_index);

            internal Node(ref BlobTreeAny tree, int index)
            {
                fixed (BlobTreeAny* ptr = &tree) _treePtr = ptr;
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