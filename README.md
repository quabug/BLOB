# BLOB
An alternative way to build BLOB for both [Unity.Entities](https://docs.unity3d.com/Packages/com.unity.entities@0.50/manual/blobs.html) and .NET.

# Usage
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

Builder comparison between *com.quabug.BLOB* and *Unity.Entities*

</td>
</tr>
</table>
