[![NuGet Badge](https://img.shields.io/nuget/v/BLOB.svg?style=flat)](https://www.nuget.org/packages/BLOB/)
[![openupm](https://img.shields.io/npm/v/com.quabug.blob?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.quabug.blob/)

# BLOB
Standalone to build BLOB for both [Unity.Entities](https://docs.unity3d.com/Packages/com.unity.entities@0.50/manual/blobs.html) and .NET.

## Usage
### Builder

<table>
  
<tr>
  <td align="center"><strong>BLOB Type</strong></td> <td align="center"><strong>com.quabug.BLOB</strong></td> <td align="center"><strong>Unity.Entities</strong></td>
</tr>
  
<tr>
<td>
  <sub>
  
`int`
  </sub>
</td>
<td>
  <sub>
    
``` c#
var builder = new ValueBuilder<int>(1);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value, Is.EqualTo(1));
```
  </sub>
</td>
<td>
  <sub>
    
``` c#
var builder = new BlobBuilder(Allocator.Temp);
builder.ConstructRoot<int>() = 1;
var blob = builder.CreateBlobAssetReference<int>(Allocator.Temp);
Assert.That(blob.Value, Is.EqualTo(1));
```
  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>
  
`BlobArray<int>`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new ArrayBuilder<int>(new [] { 1, 2, 3 });
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.ToArray(), Is.EquivalentTo(new[]{1,2,3}));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var intArray = ref builder.ConstructRoot<BlobArray<int>>();
var intArrayBuilder = builder.Allocate(ref intArray, 3);
for (var i = 0; i < 3; i++) intArrayBuilder[i] = i + 1;
var blob = builder.CreateBlobAssetReference<BlobArray<int>>(Allocator.Temp);
Assert.That(blob.Value.ToArray(), Is.EquivalentTo(new[]{1,2,3}));
```
  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>
  
`BlobString`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new StringBuilder<UTF8Encoding>("123");
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.ToString(), Is.EquivalentTo("123"));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var blobString = ref builder.ConstructRoot<BlobString>();
builder.AllocateString(ref blobString, "123");
var blob = builder.CreateBlobAssetReference<BlobString>(Allocator.Temp);
Assert.That(blob.Value.ToString(), Is.EquivalentTo("123"));
```
  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>
  
`BlobPtr<int>`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new PtrBuilderWithNewValue<int>(1);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.Value, Is.EqualTo(1));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var intPtr = ref builder.ConstructRoot<BlobPtr<int>>();
builder.Allocate(ref intPtr) = 1;
var blob = builder.CreateBlobAssetReference<BlobPtr<int>>(Allocator.Temp);
Assert.That(blob.Value.Value, Is.EqualTo(1));
```
  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>

``` c#
struct IntPtr
{
  int Int;
  BlobPtr<int> Ptr;
}
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new PtrBuilderWithNewValue<int>(1);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.Value, Is.EqualTo(1));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var intPtr = ref builder.ConstructRoot<IntPtr>();
intPtr.Int = 1;
builder.SetPointer(ref intPtr.Ptr, ref intPtr.Int);
var blob = builder.CreateBlobAssetReference<IntPtr>(Allocator.Temp);
Assert.That(blob.Value.Int, Is.EqualTo(1));
Assert.That(blob.Value.Ptr.Value, Is.EqualTo(1));
```
  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>

``` c#
struct PtrPtr
{
  BlobPtr<int> Ptr1;
  BlobPtr<int> Ptr2;
}
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new StructBuilder<PtrPtr>();
var ptrBuilder = builder.SetPointer(ref builder.Value.Ptr1, 1);
builder.SetPointer(ref builder.Value.Ptr2, ptrBuilder.ValueBuilder);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.Ptr1.Value, Is.EqualTo(1));
Assert.That(blob.Value.Ptr2.Value, Is.EqualTo(1));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var ptrPtr = ref builder.ConstructRoot<PtrPtr>();
ref var ptrValue = ref builder.Allocate(ref ptrPtr.Ptr1);
ptrValue = 1;
builder.SetPointer(ref ptrPtr.Ptr2, ref ptrValue);
var blob = builder.CreateBlobAssetReference<PtrPtr>(Allocator.Temp);
Assert.That(blob.Value.Ptr1.Value, Is.EqualTo(1));
Assert.That(blob.Value.Ptr2.Value, Is.EqualTo(1));
```
  </sub>
</td>
</tr>
  

<tr>
<td>
  <sub>

``` c#
using BlobString = 
BlobString<UTF8Encoding>;
    
struct Blob
{
  int Int;
  BlobString String;
  BlobPtr<BlobString> PtrString;
  BlobArray<int> IntArray;
}
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new StructBuilder<Blob>();
builder.SetValue(ref builder.Value.Int, 1);
var stringBuilder = builder.SetString(ref builder.Value.String, "123");
builder.SetPointer(ref builder.Value.PtrString, stringBuilder);
builder.SetArray(ref builder.Value.IntArray, new[] { 1, 2, 3 });
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.Int, Is.EqualTo(1));
Assert.That(blob.Value.String.ToString(), Is.EqualTo("123"));
Assert.That(blob.Value.PtrString.Value.ToString(), Is.EqualTo("123"));
Assert.That(blob.Value.IntArray.ToArray(), Is.EqualTo(new[]{1,2,3}));
```
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new BlobBuilder(Allocator.Temp);
ref var root = ref builder.ConstructRoot<Blob>();
root.Int = 1;
builder.AllocateString(ref root.String, "123");
builder.SetPointer(ref root.PtrString, ref root.String);
var intArrayBuilder = builder.Allocate(ref root.IntArray, 3);
for (var i = 0; i < 3; i++) intArrayBuilder[i] = i + 1;
var blob = builder.CreateBlobAssetReference<Blob>(Allocator.Temp);
Assert.That(blob.Value.Int, Is.EqualTo(1));
Assert.That(blob.Value.String.ToString(), Is.EqualTo("123"));
Assert.That(blob.Value.PtrString.Value.ToString(), Is.EqualTo("123"));
Assert.That(blob.Value.IntArray.ToArray(), Is.EqualTo(new [] {1, 2, 3}));
```
  </sub>
</td>
</tr>
    
<tr>
<td>
  <sub>

`BlobSortedArray<int, int>`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new SortedArrayBuilder<int, int>(
    new []{ (1, 123), (2, 234), (3, 345) });
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value[1], Is.EqualTo(123));
Assert.That(blob.Value[2], Is.EqualTo(234));
Assert.That(blob.Value[3], Is.EqualTo(345));
```
  </sub>
</td>
<td>
  <sub>

  </sub>
</td>
</tr>
    
<tr>
<td>
  <sub>

`BlobTree<int>`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var node = new TreeNode<int>(100, new []
{
    new TreeNode<int>(200),
    new TreeNode<int>(300)
});
var builder = new TreeBuilder<int>(node);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.FindChildrenIndices(0), Is.EqualTo(new[]{1,2}));
Assert.That(blob.Value[0], Is.EqualTo(100));
Assert.That(blob.Value[1], Is.EqualTo(200));
Assert.That(blob.Value[2], Is.EqualTo(300));
```
  </sub>
</td>
<td>
  <sub>

  </sub>
</td>
</tr>
    
<tr>
<td>
  <sub>

`BlobPtrAny`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new AnyPtrBuilder();
builder.SetValue(100);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));
    
builder.SetValue(long.MaxValue);
blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.GetValue<long>(), Is.EqualTo(long.MaxValue));
Assert.That(blob.Value.Size, Is.EqualTo(sizeof(long)));
```
  </sub>
</td>
<td>
  <sub>

  </sub>
</td>
</tr>
  
<tr>
<td>
  <sub>

`BlobArrayAny`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var builder = new AnyArrayBuilder();
builder.Add(123L);
builder.Add(456);
builder.Add(1111.0f);
builder.Add(2333.3);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.GetValue<long>(0), Is.EqualTo(123L));
Assert.That(blob.Value.GetValue<int>(1), Is.EqualTo(456));
Assert.That(blob.Value.GetValue<float>(2), Is.EqualTo(1111.0f));
Assert.That(blob.Value.GetValue<double>(3), Is.EqualTo(2333.3));
```
  </sub>
</td>
<td>
  <sub>

  </sub>
</td>
</tr>
    
    
<tr>
<td>
  <sub>

`BlobTreeAny`
  </sub>
</td>
<td>
  <sub>
  
``` c#
var intNode = new TreeNode(new ValueBuilder<int>(100));
var longNode = new TreeNode(new ValueBuilder<long>(200));
var doubleNode = new TreeNode(new ValueBuilder<double>(300));
longNode.Parent = intNode;
doubleNode.Parent = intNode;
var builder = new AnyTreeBuilder(intNode);
var blob = builder.CreateManagedBlobAssetReference();
Assert.That(blob.Value.FindChildrenIndices(0), Is.EqualTo(new[]{1,2}));
Assert.That(blob.Value[0].GetValue<int>(), Is.EqualTo(100));
Assert.That(blob.Value[1].GetValue<long>(), Is.EqualTo(200));
Assert.That(blob.Value[2].GetValue<double>(), Is.EqualTo(300));
```
  </sub>
</td>
<td>
  <sub>

  </sub>
</td>
</tr>
    
</table>
