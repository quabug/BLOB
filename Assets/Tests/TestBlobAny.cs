using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobAny
    {
        [Test]
        public void should_create_int_from_any_value_builder()
        {
            var builder = new AnyValueBuilder();
            builder.SetValue(100);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.GetValue<int>(), Is.EqualTo(100));
            Assert.That(blob.Blob.Length, Is.EqualTo(4));
        }

        [Test]
        public void should_create_long_from_any_value_builder()
        {
            var builder = new AnyValueBuilder();
            builder.SetValue(100L);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.GetValue<long>(), Is.EqualTo(100L));
            Assert.That(blob.Blob.Length, Is.EqualTo(8));
        }

        [Test]
        public void should_create_blob_any_ptr_from_generic_builder()
        {
            var builder = new AnyPtrBuilder<int>(100);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<int>(), Is.EqualTo(100));
            Assert.That(blob.Value.Size, Is.EqualTo(sizeof(int)));

            builder.SetValue(200);
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

        [Test]
        public void should_create_blob_any_array()
        {
            var builder = new AnyArrayBuilder();
            builder.Add(123L);
            builder.Add(456);
            builder.Add(1111.0f);
            builder.Add(2333.3);
            builder.Add(new PtrBuilderWithNewValue<int>(1234));
            builder.Add(new ArrayBuilder<long>(new[] { 1L, 2L, 3L }));

            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.GetValue<long>(0), Is.EqualTo(123L));
            Assert.That(blob.Value.GetValue<int>(1), Is.EqualTo(456));
            Assert.That(blob.Value.GetValue<float>(2), Is.EqualTo(1111.0f));
            Assert.That(blob.Value.GetValue<double>(3), Is.EqualTo(2333.3));

            Assert.That(blob.Value.GetValue<BlobPtr<int>>(4).Value, Is.EqualTo(1234));
            Assert.That(blob.Value.GetSize(4), Is.EqualTo(8));

            Assert.That(blob.Value.GetValue<BlobArray<long>>(5).ToArray(), Is.EquivalentTo(new [] { 1L, 2L, 3L }));
            Assert.That(blob.Value.GetSize(5), Is.EqualTo(32));
        }
    }
}