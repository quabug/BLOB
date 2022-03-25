using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Blob.Tests
{
    using BlobString = BlobString<UTF8Encoding>;
    using BlobStringBuilder = BlobStringBuilder<UTF8Encoding>;

    public class TestBlobBuilder
    {
        struct SimpleBlob
        {
            public byte Byte;
            public int Int;
            public long Long;
            public sbyte SByte;
            public float Float;
            public double Double;
            public uint UInt;
            public short Short;
        }

        [Test]
        public unsafe void should_build_blob_from_simple_blob_structure()
        {
            var builder = new BlobBuilder<SimpleBlob>();
            builder.Value.Byte = 123;
            builder.Value.Int = 234;
            builder.Value.Long = 1234321;
            builder.Value.SByte = -123;
            builder.Value.Float = 321312.123f;
            builder.Value.Double = 3213214321312.3213214321;
            builder.Value.UInt = 789378;
            builder.Value.Short = 4321;
            var blob = builder.CreateManagedBlobAssetReference();

            Assert.AreEqual(sizeof(SimpleBlob), blob.Length);
            MemCmp(ref builder.Value, blob.Blob);
        }

        private static string[] _stringTestCases =
        {
            "", "0", "123", "ajklfda",
            "夫i惹急了古v哦就立刻热情",
            "ブジェクト解決時 ゼロアロケーション"
        };

        [Test, TestCaseSource(nameof(_stringTestCases))]
        public void should_build_blob_from_string(string str)
        {
            AssertStringEqual<UTF8Encoding>(str);
            AssertStringEqual<UnicodeEncoding>(str);
            AssertStringEqual<UTF32Encoding>(str);
        }

        [Test, TestCaseSource(nameof(_stringTestCases))]
        public unsafe void should_build_unity_blob_from_string(string str)
        {
            var builder = new UnityBlobStringBuilder(str);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.AreEqual(str, blob.Value.ToString());
            var headerSize = sizeof(UnityBlobString);
            Assert.That(SubBlob(blob.Blob, headerSize, blob.Value.Length), Is.EquivalentTo(new UTF8Encoding().GetBytes(str)));
        }

        [Test]
        public void should_build_blob_from_blob_ptr()
        {
            AssertPtrEqual<int>(100);
            AssertPtrEqual<float>(200f);
            AssertPtrEqual<double>(300);
            AssertPtrEqual<long>(long.MaxValue);
            AssertPtrEqual<long>(long.MinValue);
            AssertPtrEqual<byte>(byte.MaxValue);
            AssertPtrEqual<sbyte>(sbyte.MinValue);
            AssertPtrEqual(new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678});
        }

        [Test]
        public void should_build_blob_from_blob_array()
        {
            AssertArrayEqual(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new float[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new short[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new long[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new decimal[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            AssertArrayEqual(new SimpleBlob[]
            {
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
                new SimpleBlob { Int = 123, Byte = 111, Double = 32131, Float = 31111f, Long = 33789479, SByte = -33, Short = 4321, UInt = 3123678},
            });
        }

        struct ComplexBlob
        {
            public BlobPtr<int> IntPtr;
            public byte Byte;
            public BlobArray<float> FloatArray;
            public BlobArray<BlobArray<BlobPtr<BlobString>>> StringArray2Ptr;
            public BlobString String;
            public long Long;
            public int Int;
            public BlobPtr<BlobString<UnicodeEncoding>> StringPtr;
        }

        [Test]
        public void should_build_blob_from_complex_blob_structure()
        {
            var builder = new BlobBuilder<ComplexBlob>();
            builder.SetBuilder(ref builder.Value.IntPtr, new BlobPtrBuilder<int>(123));
            builder.Value.Byte = 222;
            builder.SetBuilder(ref builder.Value.FloatArray, new BlobArrayBuilder<float>(new float[] { 1, 2, 3, 4, 5 }));
            var string2 = new[]
            {
                new[] { "fdjkl", "fjdklfd", "uerwuiorew", "fvjkfdauio", "放大镜看浪费大家快乐", "发动机看来放大12321fjdklfdas" },
                new[] { "fjdklfd", "uerwuiorew", "fvjkfdauio", "放大镜看浪费大家快乐", "发动机看来放大12321fjdklfdas" },
                new[] { "uerwuiorew", "fvjkfdauio", "放大镜看浪费大家快乐", "发动机看来放大12321fjdklfdas" },
            };
            var string2Builder = new BlobArrayBuilder<BlobArray<BlobPtr<BlobString>>>( string2
                .Select(stringArray => stringArray.Select(str => new BlobPtrBuilder<BlobString>(new BlobStringBuilder(str))))
                .Select(builders => new BlobArrayBuilder<BlobPtr<BlobString>>(builders))
            );
            builder.SetBuilder(ref builder.Value.StringArray2Ptr, string2Builder);
            builder.SetBuilder(ref builder.Value.String, new BlobStringBuilder("rfeuivjl, 放大镜考过托福i哦热情"));
            builder.Value.Long = 31789457893;
            builder.Value.Int = 13278;
            builder.SetBuilder(ref builder.Value.StringPtr, new BlobPtrBuilder<BlobString<UnicodeEncoding>>(new BlobStringBuilder<UnicodeEncoding>("放大镜fdjakfldsauiroew看热舞哦i13278941fdafjdaksl")));

            var blob = builder.CreateManagedBlobAssetReference();
            Assert.AreEqual(123, blob.Value.IntPtr.Value);
            Assert.AreEqual(222, blob.Value.Byte);
            Assert.That(blob.Value.FloatArray.ToArray(), Is.EquivalentTo(new float[] { 1, 2, 3, 4, 5 }));
            var string2Flat = string2.SelectMany(s => s).ToArray();
            var index = 0;
            for (var i = 0; i < blob.Value.StringArray2Ptr.Length; i++)
            {
                ref var strings = ref blob.Value.StringArray2Ptr[i];
                for (var x = 0; x < strings.Length; x++)
                {
                    Assert.AreEqual(string2Flat[index], strings[x].Value.ToString());
                    index++;
                }
            }
            Assert.AreEqual("rfeuivjl, 放大镜考过托福i哦热情", blob.Value.String.ToString());
            Assert.AreEqual(31789457893, blob.Value.Long);
            Assert.AreEqual(13278, blob.Value.Int);
            Assert.AreEqual("放大镜fdjakfldsauiroew看热舞哦i13278941fdafjdaksl", blob.Value.StringPtr.Value.ToString());
        }

        void AssertArrayEqual<T>(T[] array) where T : unmanaged
        {
            var builder = new BlobArrayBuilder<T>(array);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.AreEqual(array.Length, blob.Value.Length);
            for (var i = 0; i < array.Length; i++) MemCmp(ref array[i], ref blob.Value[i]);
            Assert.That(array, Is.EquivalentTo(blob.Value.ToArray()));
        }

        unsafe void AssertPtrEqual<T>(T value) where T : unmanaged
        {
            var builder = new BlobPtrBuilder<T>(value);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.AreEqual(value, blob.Value.Value);
            MemCmp(ref value, SubBlob(blob.Blob, sizeof(BlobPtr<T>), sizeof(T)));
        }

        unsafe void AssertStringEqual<TEncoding>(string str) where TEncoding : Encoding, new()
        {
            var builder = new BlobStringBuilder<TEncoding>(str);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.AreEqual(str, blob.Value.ToString());
            var headerSize = sizeof(BlobString<TEncoding>);
            Assert.That(SubBlob(blob.Blob, headerSize, blob.Value.Length), Is.EquivalentTo(new TEncoding().GetBytes(str)));
        }

        byte[] SubBlob(byte[] blob, int startIndex, int length)
        {
            var sub = new byte[length];
            Array.Copy(blob, startIndex, sub, 0, sub.Length);
            return sub;
        }

        byte[] SubBlob(byte[] blob, int startIndex)
        {
            return SubBlob(blob, startIndex, blob.Length - startIndex);
        }

        unsafe void MemCmp<T>(ref T expected, ref T actual) where T : unmanaged
        {
            MemCmp(ref expected, ref actual, sizeof(T));
        }

        unsafe void MemCmp<T>(ref T expected, byte[] actual) where T : unmanaged
        {
            Assert.AreEqual(sizeof(T), actual.Length);
            Assert.That(actual, Is.EquivalentTo(ToBinary(ref expected, sizeof(T))));
        }

        void MemCmp<T>(ref T expected, ref T actual, int size) where T : unmanaged
        {
            Assert.That(ToBinary(ref actual, size), Is.EquivalentTo(ToBinary(ref expected, size)));
        }

        unsafe byte[] ToBinary<T>(ref T value, int size) where T : unmanaged
        {
            if (size < 1) throw new ArgumentOutOfRangeException();

            var binary = new byte[size];
            fixed(void* source = &value)
            fixed (void* destination = &binary[0])
            {
                Buffer.MemoryCopy(source, destination, size, size);
            }
            return binary;
        }
    }
}
