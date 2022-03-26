using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Blob.Tests
{
    using BlobArray = Unity.Entities.BlobArray<SimpleBlob>;
    using BlobArray2 = Unity.Entities.BlobArray<Unity.Entities.BlobArray<SimpleBlob>>;
    using BlobPtr = Unity.Entities.BlobPtr<SimpleBlob>;
    using BlobPtr2 = Unity.Entities.BlobPtr<Unity.Entities.BlobPtr<SimpleBlob>>;
    using BlobArrayPtr = Unity.Entities.BlobArray<Unity.Entities.BlobPtr<SimpleBlob>>;
    using BlobPtrArray = Unity.Entities.BlobPtr<Unity.Entities.BlobArray<SimpleBlob>>;

    struct SimpleBlob
    {
        public int Int;
        public long Long;
        public float Float;
        public double Double;
    }

    public class TestFormatWithUnityBlob
    {
        private BlobBuilder unityBuilder;

        [SetUp]
        public void SetUp()
        {
            unityBuilder = new BlobBuilder(Allocator.Temp);
        }

        [TearDown]
        public void TearDown()
        {
            unityBuilder.Dispose();
        }

        [Test]
        public void should_have_same_blob_for_simple_value()
        {
            ref var unityData = ref unityBuilder.ConstructRoot<SimpleBlob>();
            unityData.Int = 1;
            unityData.Long = 2;
            unityData.Float = 3;
            unityData.Double = 4;
            var unityBlob = unityBuilder.CreateBlobAssetReference<SimpleBlob>(Allocator.Temp);

            var blobBuilder = new ValueBuilder<SimpleBlob>(unityBlob.Value);
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Ignore("bug report: https://fogbugz.unity3d.com/default.asp?1413355_hgspkf51e4ioflql")]
        [Test]
        public void should_have_same_blob_for_blob_ptr_value()
        {
            ref var unityDataPtr = ref unityBuilder.ConstructRoot<BlobPtr>();
            ref var unityData = ref unityBuilder.Allocate(ref unityDataPtr);
            unityData.Int = 1;
            unityData.Long = 2;
            unityData.Float = 3;
            unityData.Double = 4;
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobPtr>(Allocator.Temp);

            var blobBuilder = new UnityBlobPtrBuilder<SimpleBlob>(unityBlob.Value.Value);
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_ptr_int()
        {
            ref var unityDataPtr = ref unityBuilder.ConstructRoot<Unity.Entities.BlobPtr<int>>();
            unityBuilder.Allocate(ref unityDataPtr) = 999;
            var unityBlob = unityBuilder.CreateBlobAssetReference<Unity.Entities.BlobPtr<int>>(Allocator.Temp);

            var blobBuilder = new UnityBlobPtrBuilder<int>(999);
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_array_value()
        {
            const int length = 10;
            ref var unityDataArray = ref unityBuilder.ConstructRoot<BlobArray>();
            var arrayBuilder = unityBuilder.Allocate(ref unityDataArray, length);
            for (var i = 0; i < length; i++)
            {
                arrayBuilder[i].Int = 10 * i + 1;
                arrayBuilder[i].Long = 10 * i + 2;
                arrayBuilder[i].Float = 10 * i + 3;
                arrayBuilder[i].Double = 10 * i + 4;
            }
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobArray>(Allocator.Temp);

            var blobBuilder = new UnityBlobArrayBuilder<SimpleBlob>(unityBlob.Value.ToArray());
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_ptr_2()
        {
            ref var unityDataPtrPtr = ref unityBuilder.ConstructRoot<BlobPtr2>();
            ref var unityDataPtr = ref unityBuilder.Allocate(ref unityDataPtrPtr);
            ref var unityData = ref unityBuilder.Allocate(ref unityDataPtr);
            unityData.Int = 1;
            unityData.Long = 2;
            unityData.Float = 3;
            unityData.Double = 4;
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobPtr2>(Allocator.Temp);

            var blobPtrBuilder = new UnityBlobPtrBuilder<SimpleBlob>(unityBlob.Value.Value.Value);
            var blobPtr2Builder = new UnityBlobPtrBuilder<BlobPtr>(blobPtrBuilder);
            var data = blobPtr2Builder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_array_2()
        {
            const int length = 5;
            ref var unityDataArray = ref unityBuilder.ConstructRoot<BlobArray2>();
            var array2Builder = unityBuilder.Allocate(ref unityDataArray, length);
            for (var i = 0; i < length; i++)
            {
                var arrayBuilder = unityBuilder.Allocate(ref array2Builder[i], length);
                for (var j = 0; j < length; j++)
                {
                    arrayBuilder[j].Int = 10 * i * j + 1;
                    arrayBuilder[j].Long = 10 * i * j + 2;
                    arrayBuilder[j].Float = 10 * i * j + 3;
                    arrayBuilder[j].Double = 10 * i * j + 4;
                }
            }
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobArray2>(Allocator.Temp);

            var arrayBuilders = Enumerable.Range(0, length)
                .Select(index => new UnityBlobArrayBuilder<SimpleBlob>(unityBlob.Value[index].ToArray()))
            ;
            var blobBuilder = new UnityBlobArrayBuilder<BlobArray>(arrayBuilders);
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Test]
        public void should_have_same_blob_for_blob_array_ptr()
        {
            const int length = 10;
            ref var unityDataArray = ref unityBuilder.ConstructRoot<BlobArrayPtr>();
            var arrayBuilder = unityBuilder.Allocate(ref unityDataArray, length);
            for (var i = 0; i < length; i++)
            {
                ref var dataBuilder = ref unityBuilder.Allocate(ref arrayBuilder[i]);
                dataBuilder.Int = 10 * i + 1;
                dataBuilder.Long = 10 * i + 2;
                dataBuilder.Float = 10 * i + 3;
                dataBuilder.Double = 10 * i + 4;
            }
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobArrayPtr>(Allocator.Temp);

            var builders = Enumerable.Range(0, length)
                .Select(index => unityBlob.Value[index].Value)
                .Select(data => new UnityBlobPtrBuilder<SimpleBlob>(data))
                .ToArray()
            ;
            var blobBuilder = new UnityBlobArrayBuilder<BlobPtr>(builders);
            var data = blobBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        [Ignore("bug report: https://fogbugz.unity3d.com/default.asp?1413366_9iul87jv64suk5tv")]
        [Test]
        public void should_have_same_blob_for_blob_ptr_array()
        {
            const int length = 10;
            ref var unityDataArray = ref unityBuilder.ConstructRoot<BlobPtrArray>();
            ref var unityBlobArray = ref unityBuilder.Allocate(ref unityDataArray);
            var arrayBuilder = unityBuilder.Allocate(ref unityBlobArray, length);
            for (var i = 0; i < length; i++)
            {
                arrayBuilder[i].Int = 10 * i + 1;
                arrayBuilder[i].Long = 10 * i + 2;
                arrayBuilder[i].Float = 10 * i + 3;
                arrayBuilder[i].Double = 10 * i + 4;
            }
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobPtrArray>(Allocator.Temp);

            var values = Enumerable.Range(0, length)
                .Select(index => unityBlob.Value.Value[index])
                .ToArray()
            ;
            var blobArrayBuilder = new UnityBlobArrayBuilder<SimpleBlob>(values);
            var blobPtrBuilder = new UnityBlobPtrBuilder<BlobArray>(blobArrayBuilder);
            var data = blobPtrBuilder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, data);
        }

        private static string[] _stringTestCases =
        {
            "", "0", "123", "ajklfda",
            // TODO: track serialization bug of `BlobString`
            //       https://fogbugz.unity3d.com/default.asp?1413322_vsglij0ujncq6kfi
            // "夫i惹急了古v哦就立刻热情",
            // "ブジェクト解決時 ゼロアロケーション"
        };

        [Test, TestCaseSource(nameof(_stringTestCases))]
        public void should_have_same_blob_for_blob_string(string str)
        {
            ref var blobString = ref unityBuilder.ConstructRoot<BlobString>();
            unityBuilder.AllocateString(ref blobString, str);
            var unityBlob = unityBuilder.CreateBlobAssetReference<BlobString>(Allocator.Temp);
            var blob = new UnityBlobStringBuilder(str).CreateUnityBlobAssetReference();
            Assert.AreEqual(unityBlob.Value.ToString(), blob.Value.ToString());
            AssertBlobEqual(unityBlob, blob);
        }

        struct ComplexBlob
        {
            public Unity.Entities.BlobPtr<int> IntPtr;
            public Unity.Entities.BlobString String;
            public int Int;
            public Unity.Entities.BlobArray<int> IntArray;
            public float Float;
            // bug report: https://fogbugz.unity3d.com/default.asp?1413355_hgspkf51e4ioflql
            // public long Long;
            public Unity.Entities.BlobPtr<Unity.Entities.BlobArray<float>> FloatArrayPtr;
        }

        [Test]
        public void should_have_same_blob_for_complex_blob()
        {
            ref var unityBlobRoot = ref unityBuilder.ConstructRoot<ComplexBlob>();
            unityBuilder.AllocateString(ref unityBlobRoot.String, "12345");
            unityBlobRoot.Int = 1;
            unityBuilder.SetPointer(ref unityBlobRoot.IntPtr, ref unityBlobRoot.Int);
            var longArrayBuilder = unityBuilder.Allocate(ref unityBlobRoot.IntArray, 3);
            for (var i = 0; i < 3; i++) longArrayBuilder[i] = 30 + i;
            unityBlobRoot.Float = 777f;
            ref var floatArray = ref unityBuilder.Allocate(ref unityBlobRoot.FloatArrayPtr);
            var floatArrayBuilder = unityBuilder.Allocate(ref floatArray, 3);
            for (var i = 0; i < 3; i++) floatArrayBuilder[i] = 100 + i;
            var unityBlob = unityBuilder.CreateBlobAssetReference<ComplexBlob>(Allocator.Temp);

            var builder = new StructBuilder<ComplexBlob>();
            builder.SetBuilder(ref builder.Value.String, new UnityBlobStringBuilder(unityBlob.Value.String.ToString()));
            builder.SetBuilder(ref builder.Value.Int, unityBlob.Value.Int);
            builder.SetBuilder(ref builder.Value.IntPtr, new UnityBlobRefPtrBuilder<int>(builder.GetBuilder(ref builder.Value.Int)));
            builder.SetBuilder(ref builder.Value.IntArray, new UnityBlobArrayBuilder<int>(unityBlob.Value.IntArray.ToArray()));
            builder.SetBuilder(ref builder.Value.Float, unityBlob.Value.Float);
            builder.SetBuilder(ref builder.Value.FloatArrayPtr, new UnityBlobPtrBuilder<Unity.Entities.BlobArray<float>>( new UnityBlobArrayBuilder<float>(unityBlob.Value.FloatArrayPtr.Value.ToArray())));
            var blob = builder.CreateUnityBlobAssetReference();

            AssertBlobEqual(unityBlob, blob);
        }

        static unsafe void AssertBlobEqual<T>(BlobAssetReference<T> expected, BlobAssetReference<T> actual)
            where T : unmanaged
        {
            Assert.AreEqual(expected.GetLength(), actual.GetLength());
            var expectedBinary = ToByteArray(expected);
            var actualBinary = ToByteArray(actual);
            Assert.That(actualBinary, Is.EquivalentTo(expectedBinary));

            byte[] ToByteArray(BlobAssetReference<T> blob)
            {
                var array = new byte[blob.GetLength()];
                var arrayPtr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var gcHandler);
                try
                {
                    UnsafeUtility.MemCpy(arrayPtr, blob.GetUnsafePtr(), blob.GetLength());
                }
                finally
                {
                    UnsafeUtility.ReleaseGCObject(gcHandler);
                }
                return array;
            }
        }
    }

}