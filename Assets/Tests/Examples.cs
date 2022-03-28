using System.Text;
using NUnit.Framework;

#if UNITY_BLOB
using Unity.Collections;
using Unity.Entities;
#endif

namespace Blob.Tests
{
    public class Examples
    {
        [Test]
        public void int_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            unityBuilder.ConstructRoot<int>() = 1;
            var unityBlob = unityBuilder.CreateBlobAssetReference<int>(Allocator.Temp);
            Assert.That(unityBlob.Value, Is.EqualTo(1));
#endif

            var builder = new ValueBuilder<int>(1);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value, Is.EqualTo(1));
        }

        [Test]
        public void int_array_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityIntArray = ref unityBuilder.ConstructRoot<Unity.Entities.BlobArray<int>>();
            var unityIntArrayBuilder = unityBuilder.Allocate(ref unityIntArray, 3);
            for (var i = 0; i < 3; i++) unityIntArrayBuilder[i] = i + 1;
            var unityBlob = unityBuilder.CreateBlobAssetReference<Unity.Entities.BlobArray<int>>(Allocator.Temp);
            Assert.That(unityBlob.Value.ToArray(), Is.EquivalentTo(new [] { 1, 2, 3}));
#endif

            var builder = new ArrayBuilder<int>(new [] { 1, 2, 3 });
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.ToArray(), Is.EquivalentTo(new [] { 1, 2, 3}));
        }

        [Test]
        public void string_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityString = ref unityBuilder.ConstructRoot<BlobString>();
            unityBuilder.AllocateString(ref unityString, "123");
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobString>(Allocator.Temp);
            Assert.That(unityBlob.Value.ToString(), Is.EquivalentTo("123"));
#endif

            var builder = new StringBuilder<UTF8Encoding>("123");
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.ToString(), Is.EquivalentTo("123"));
        }

        [Test]
        public void int_ptr_with_new_value_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new Unity.Entities.BlobBuilder(Allocator.Temp);
            ref var unityIntPtr = ref unityBuilder.ConstructRoot<Unity.Entities.BlobPtr<int>>();
            unityBuilder.Allocate(ref unityIntPtr) = 1;
            var unityBlob = unityBuilder.CreateBlobAssetReference<Unity.Entities.BlobPtr<int>>(Allocator.Temp);
            Assert.That(unityBlob.Value.Value, Is.EqualTo(1));
#endif

            var builder = new PtrBuilderWithNewValue<int>(1);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Value, Is.EqualTo(1));
        }

#if UNITY_BLOB
        struct UnityIntPtr
        {
            public int Int;
            public Unity.Entities.BlobPtr<int> Ptr;
        }
#endif

        struct IntPtr
        {
            public int Int;
            public BlobPtr<int> Ptr;
        }

        [Test]
        public void int_ptr_to_int_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityIntPtr = ref unityBuilder.ConstructRoot<UnityIntPtr>();
            unityIntPtr.Int = 1;
            unityBuilder.SetPointer(ref unityIntPtr.Ptr, ref unityIntPtr.Int);
            var unityBlob = unityBuilder.CreateBlobAssetReference<UnityIntPtr>(Allocator.Temp);
            Assert.That(unityBlob.Value.Int, Is.EqualTo(1));
            Assert.That(unityBlob.Value.Ptr.Value, Is.EqualTo(1));
#endif

            var builder = new StructBuilder<IntPtr>();
            var valueBuilder = builder.SetValue(ref builder.Value.Int, 1);
            builder.SetPointer(ref builder.Value.Ptr, valueBuilder);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Int, Is.EqualTo(1));
            Assert.That(blob.Value.Ptr.Value, Is.EqualTo(1));
        }

#if UNITY_BLOB
        struct UnityPtrPtr
        {
            public Unity.Entities.BlobPtr<int> Ptr1;
            public Unity.Entities.BlobPtr<int> Ptr2;
        }
#endif

        struct PtrPtr
        {
            public BlobPtr<int> Ptr1;
            public BlobPtr<int> Ptr2;
        }

        [Test]
        public void int_ptr_to_another_ptr_value_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityPtrPtr = ref unityBuilder.ConstructRoot<UnityPtrPtr>();
            ref var ptrValue = ref unityBuilder.Allocate(ref unityPtrPtr.Ptr1);
            ptrValue = 1;
            unityBuilder.SetPointer(ref unityPtrPtr.Ptr2, ref ptrValue);
            var unityBlob = unityBuilder.CreateBlobAssetReference<UnityPtrPtr>(Allocator.Temp);
            Assert.That(unityBlob.Value.Ptr1.Value, Is.EqualTo(1));
            Assert.That(unityBlob.Value.Ptr2.Value, Is.EqualTo(1));
#endif

            var builder = new StructBuilder<PtrPtr>();
            var valueBuilder = builder.SetPointer(ref builder.Value.Ptr1, 1).ValueBuilder;
            builder.SetPointer(ref builder.Value.Ptr2, valueBuilder);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Ptr1.Value, Is.EqualTo(1));
            Assert.That(blob.Value.Ptr2.Value, Is.EqualTo(1));
        }

#if UNITY_BLOB
        struct UnityBlob
        {
            public int Int;
            public BlobString String;
            public Unity.Entities.BlobPtr<BlobString> PtrString;
            public Unity.Entities.BlobArray<int> IntArray;
        }
#endif

        struct Blob
        {
            public int Int;
            public BlobString<UTF8Encoding> String;
            public BlobPtr<BlobString<UTF8Encoding>> PtrString;
            public BlobArray<int> IntArray;
        }

        [Test]
        public void struct_blob()
        {
#if UNITY_BLOB
            var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityBlobRoot = ref unityBuilder.ConstructRoot<UnityBlob>();
            unityBlobRoot.Int = 1;
            unityBuilder.AllocateString(ref unityBlobRoot.String, "123");
            unityBuilder.SetPointer(ref unityBlobRoot.PtrString, ref unityBlobRoot.String);
            var intArrayBuilder = unityBuilder.Allocate(ref unityBlobRoot.IntArray, 3);
            for (var i = 0; i < 3; i++) intArrayBuilder[i] = i + 1;
            var unityBlob = unityBuilder.CreateBlobAssetReference<UnityBlob>(Allocator.Temp);
            Assert.That(unityBlob.Value.Int, Is.EqualTo(1));
            Assert.That(unityBlob.Value.String.ToString(), Is.EqualTo("123"));
            Assert.That(unityBlob.Value.PtrString.Value.ToString(), Is.EqualTo("123"));
            Assert.That(unityBlob.Value.IntArray.ToArray(), Is.EqualTo(new [] {1, 2, 3}));
#endif

            var builder = new StructBuilder<Blob>();
            builder.SetValue(ref builder.Value.Int, 1);
            var stringBuilder = builder.SetString(ref builder.Value.String, "123");
            builder.SetPointer(ref builder.Value.PtrString, stringBuilder);
            builder.SetArray(ref builder.Value.IntArray, new[] { 1, 2, 3 });
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Int, Is.EqualTo(1));
            Assert.That(blob.Value.String.ToString(), Is.EqualTo("123"));
            Assert.That(blob.Value.PtrString.Value.ToString(), Is.EqualTo("123"));
            Assert.That(blob.Value.IntArray.ToArray(), Is.EqualTo(new [] {1, 2, 3}));
        }
    }
}