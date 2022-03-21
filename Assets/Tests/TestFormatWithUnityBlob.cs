using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Tests
{

    public class TestFormatWithUnityBlob
    {
        struct PrimitiveBlob
        {
            public int Int;
            public long Long;
            public float Float;
            public double Double;
        }

        [Test]
        public void should_have_same_blob_for_simple_value()
        {
            using var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityData = ref unityBuilder.ConstructRoot<PrimitiveBlob>();
            unityData.Int = 1;
            unityData.Long = 2;
            unityData.Float = 3;
            unityData.Double = 4;
            var unityBlob = unityBuilder.CreateBlobAssetReference<PrimitiveBlob>(Allocator.Temp);

            var blobBuilder = new BlobBuilder<PrimitiveBlob>(unityBlob.Value);
            using var stream = new MemoryStream();
            blobBuilder.Build(stream, 0, 0);
            var data = BlobAssetReference<PrimitiveBlob>.Create(stream.ToArray()).Value;

            Assert.AreEqual(unityData, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_ptr_value()
        {
            using var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityDataPtr = ref unityBuilder.ConstructRoot<Unity.Entities.BlobPtr<PrimitiveBlob>>();
            ref var unityData = ref unityBuilder.Allocate(ref unityDataPtr);
            unityData.Int = 1;
            unityData.Long = 2;
            unityData.Float = 3;
            unityData.Double = 4;
            var unityBlob = unityBuilder.CreateBlobAssetReference<Unity.Entities.BlobPtr<PrimitiveBlob>>(Allocator.Temp);

            var blobBuilder = new BlobPtrBuilder<PrimitiveBlob>(unityBlob.Value.Value);
            using var stream = new MemoryStream();
            blobBuilder.Build(stream, 0, 0);
            var data = BlobAssetReference<Unity.Entities.BlobPtr<PrimitiveBlob>>.Create(stream.ToArray()).Value.Value;

            Assert.AreEqual(unityData, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_array_value()
        {
            const int length = 10;
            using var unityBuilder = new BlobBuilder(Allocator.Temp);
            ref var unityDataArray = ref unityBuilder.ConstructRoot<Unity.Entities.BlobArray<PrimitiveBlob>>();
            var arrayBuilder = unityBuilder.Allocate(ref unityDataArray, length);
            for (var i = 0; i < length; i++)
            {
                arrayBuilder[i].Int = 10 * i + 1;
                arrayBuilder[i].Long = 10 * i + 2;
                arrayBuilder[i].Float = 10 * i + 3;
                arrayBuilder[i].Double = 10 * i + 4;
            }
            var unityBlob = unityBuilder.CreateBlobAssetReference<Unity.Entities.BlobArray<PrimitiveBlob>>(Allocator.Temp);

            var blobBuilder = new BlobArrayBuilder<PrimitiveBlob>(unityBlob.Value.ToArray());
            using var stream = new MemoryStream();
            blobBuilder.Build(stream, 0, 0);
            var data = BlobAssetReference<Unity.Entities.BlobArray<PrimitiveBlob>>.Create(stream.ToArray());

            var unityArray = unityBlob.Value.ToArray();
            var array = data.Value.ToArray();

            Assert.That(array, Is.EqualTo(unityArray));
        }
    }

}