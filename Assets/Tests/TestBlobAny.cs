using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobAnyPtr
    {
        [Test]
        public void should_create_blob_any_ptr_from_generic_builder()
        {
            var builder = new AnyPtrBuilder<int>(100);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
            Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));

            builder.Value = 200;
            blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(200));
            Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));
        }

        [Test]
        public void should_create_blob_any_ptr_from_any_builder()
        {
            var builder = new AnyPtrBuilder();
            builder.SetValue(100);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
            Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));

            builder.SetValue(long.MaxValue);
            blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<long>(), Is.EqualTo(long.MaxValue));
            Assert.That(blob.Value.Size, Is.EqualTo(sizeof(long)));
        }
    }
    //
    // public class TestBlobAnyArray
    // {
    //     [Test]
    //     public void should_create_blob_any_ptr_from_generic_builder()
    //     {
    //         var builder = new AnyPtrBuilder<int>(100);
    //         var blob = builder.CreateManagedBlobAssetReference();
    //         Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
    //         Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));
    //
    //         builder.Value = 200;
    //         blob = builder.CreateManagedBlobAssetReference();
    //         Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(200));
    //         Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));
    //     }
    //
    //     [Test]
    //     public void should_create_blob_any_ptr_from_any_builder()
    //     {
    //         var builder = new AnyPtrBuilder();
    //         builder.SetValue(100);
    //         var blob = builder.CreateManagedBlobAssetReference();
    //         Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
    //         Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));
    //
    //         builder.SetValue(long.MaxValue);
    //         blob = builder.CreateManagedBlobAssetReference();
    //         Assert.That(blob.Value.GetValue<long>(), Is.EqualTo(long.MaxValue));
    //         Assert.That(blob.Value.Size, Is.EqualTo(sizeof(long)));
    //     }
    // }
    //
    // public class AnyArrayBuilder : Builder<BlobAnyArray>
    // {
    //     private readonly List<(byte[] data, int alignment)> _valueList = new List<(byte[],int)>();
    //
    //     public int Count => _valueList.Count;
    //
    //     public void Insert<T>(int index, T item) where T : unmanaged
    //     {
    //
    //     }
    //
    //     public void Add<T>(T item) where T : unmanaged
    //     {
    //
    //     }
    //
    //     public void RemoveAt(int index)
    //     {
    //         _valueList.RemoveAt(index);
    //     }
    //
    //     public void Clear()
    //     {
    //         _valueList.Clear();
    //     }
    //
    //     protected override void BuildImpl(IBlobStream stream)
    //     {
    //     }
    //
    //     private byte[] ToBytes<T>(T value) where T : unmanaged
    //     {
    //
    //     }
    // }
    //
    // public unsafe struct BlobAnyArray
    // {
    //     internal BlobArray<int> Offsets; // Offsets.Last == Data.Length
    //     internal BlobArray<byte> Data;
    //
    //     public int Length => Offsets.Length - 1;
    //
    //     public int GetOffset(int index)
    //     {
    //         return Offsets[index];
    //     }
    //
    //     public ref T GetValue<T>(int index) where T : unmanaged
    //     {
    //         return ref *GetUnsafeValuePtr<T>(index);
    //     }
    //
    //     public T* GetUnsafeValuePtr<T>(int index) where T : unmanaged
    //     {
    //         if (sizeof(T) != GetSize(index)) throw new ArgumentException("invalid generic parameter");
    //         return (T*)(Data.UnsafePtr + Offsets[index]);
    //     }
    //
    //     public int GetSize(int index)
    //     {
    //         return Offsets[index + 1] - Offsets[index];
    //     }
    // }
    //
    // public struct BlobAnyTree
    // {
    //     internal BlobArray<int> EndIndices;
    //     internal BlobArray<int> Offsets; // Offsets.Last == Data.Length
    //     internal BlobArray<byte> Data;
    //
    //     public Node this[int index] => new Node(ref this, index);
    //     public int Length => EndIndices.Length;
    //
    //     public readonly unsafe ref struct Node
    //     {
    //         private readonly BlobAnyTree* _treePtr;
    //         private readonly int _index;
    //
    //         public ref BlobAnyTree Tree => ref *_treePtr;
    //         public int Size => Tree.Offsets[_index + 1] - Offset;
    //         public void* UnsafePtr => Tree.Data.UnsafePtr + Offset;
    //
    //         public T* GetValuePtr<T>() where T : unmanaged
    //         {
    //             if (sizeof(T) != Size) throw new ArgumentException("invalid generic parameter");
    //             return (T*)UnsafePtr;
    //         }
    //
    //         public ref T GetValue<T>() where T : unmanaged => ref *GetValuePtr<T>();
    //         public int Index => _index;
    //         public int EndIndex => Tree.EndIndices[_index];
    //         public int Offset => Tree.Offsets[_index];
    //
    //         internal Node(ref BlobAnyTree tree, int index)
    //         {
    //             fixed (BlobAnyTree* ptr = &tree) _treePtr = ptr;
    //             _index = index;
    //         }
    //
    //         public int FindParentIndex() => Tree.FindParentIndex(Index);
    //
    //         public List<int> FindAncestorsIndices() => Tree.FindAncestorsIndices(Index);
    //         public List<int> FindChildrenIndices() => Tree.FindChildrenIndices(Index);
    //         public List<int> FindDescendantsIndices() => Tree.FindDescendantsIndices(Index);
    //
    //         public void FindAncestorsIndices(ICollection<int> output) => Tree.FindAncestorsIndices(Index, output);
    //         public void FindChildrenIndices(ICollection<int> output) => Tree.FindChildrenIndices(Index, output);
    //         public void FindDescendantsIndices(ICollection<int> output) => Tree.FindDescendantsIndices(Index, output);
    //
    //         public void ForEachAncestors(NodeAction action) => Tree.ForEachAncestors(Index, action);
    //         public void ForEachChildren(NodeAction action) => Tree.ForEachChildren(Index, action);
    //         public void ForEachDescendants(NodeAction action) => Tree.ForEachDescendants(Index, action);
    //     }
    //
    //     public int FindParentIndex(int index)
    //     {
    //         var endIndex = EndIndices[index];
    //         for (var i = index - 1; i >= 0; i--)
    //         {
    //             if (EndIndices[i] >= endIndex) return i;
    //         }
    //         return -1;
    //     }
    //
    //     public List<int> FindAncestorsIndices(int index)
    //     {
    //         var indices = new List<int>();
    //         FindAncestorsIndices(index, indices);
    //         return indices;
    //     }
    //
    //     public List<int> FindChildrenIndices(int index)
    //     {
    //         var indices = new List<int>();
    //         FindChildrenIndices(index, indices);
    //         return indices;
    //     }
    //
    //     public List<int> FindDescendantsIndices(int index)
    //     {
    //         var indices = new List<int>();
    //         FindDescendantsIndices(index, indices);
    //         return indices;
    //     }
    //
    //     public void FindAncestorsIndices(int index, ICollection<int> output)
    //     {
    //         ForEachAncestors(index, (in Node node) => output.Add(node.Index));
    //     }
    //
    //     public void FindChildrenIndices(int index, ICollection<int> output)
    //     {
    //         ForEachChildren(index, (in Node node) => output.Add(node.Index));
    //     }
    //
    //     public void FindDescendantsIndices(int index, ICollection<int> output)
    //     {
    //         ForEachDescendants(index, (in Node node) => output.Add(node.Index));
    //     }
    //
    //     public delegate void NodeAction(in Node node);
    //
    //     public void ForEachAncestors(int index, NodeAction action)
    //     {
    //         var endIndex = EndIndices[index];
    //         for (var i = index - 1; i >= 0; i--)
    //         {
    //             if (EndIndices[i] >= endIndex)
    //             {
    //                 action(this[i]);
    //                 endIndex = EndIndices[i];
    //             }
    //         }
    //     }
    //
    //     public void ForEachChildren(int index, NodeAction action)
    //     {
    //         var endIndex = EndIndices[index];
    //         var childIndex = index + 1;
    //         while (childIndex < endIndex)
    //         {
    //             action(this[childIndex]);
    //             childIndex = EndIndices[childIndex];
    //         }
    //     }
    //
    //     public void ForEachDescendants(int index, NodeAction action)
    //     {
    //         var endIndex = EndIndices[index];
    //         var startIndex = index + 1;
    //         for (var i = startIndex; i < endIndex; i++) action(this[i]);
    //     }
    // }
}