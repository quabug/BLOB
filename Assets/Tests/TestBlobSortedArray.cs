using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestBlobSortedArray
    {
        readonly struct Key : IEquatable<Key>
        {
            public readonly int Value;

            public Key(int value)
            {
                Value = value;
            }

            public bool Equals(Key other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Value % 100;
            }

            public static implicit operator Key(int value) => new Key(value);
        }

        [Test]
        public void should_create_empty_sorted_array()
        {
            var builder = new SortedArrayBuilder<int, int>();
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(0));
        }

        [Test]
        public void should_create_sorted_array_from_dictionary()
        {
            var map = new Dictionary<int, int>
            {
                {1, 1},
                {2, 2},
                {100, 100},
                {101, 101},
                {102, 102},
                {103, 103},
                {3, 3},
                {1003, 1003},
                {1001, 1001},
                {1005, 1005},
            };

            var builder = new SortedArrayBuilder<int, int>(map);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(map.Count));
            foreach (var pair in map) Assert.That(blob.Value[pair.Key], Is.EqualTo(pair.Value), $"key = {pair.Key}");
        }

        [Test]
        public void should_create_sorted_array_from_dictionary_with_duplicated_hash_code()
        {
            var map = new Dictionary<Key, int>
            {
                {1, 1},
                {2, 2},
                {100, 100},
                {101, 101},
                {102, 102},
                {103, 103},
                {3, 3},
                {1003, 1003},
                {1001, 1001},
                {1005, 1005},
            };

            var builder = new SortedArrayBuilder<Key, int>(map);
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(map.Count));
            foreach (var pair in map) Assert.That(blob.Value[pair.Key], Is.EqualTo(pair.Value), $"key = {pair.Key.Value}");
        }

        [Test]
        public void should_create_sorted_array_from_random_array([Random(50)] int seed)
        {
            var random = new Random(seed);
            var randomArray = Enumerable.Range(0, 1000).Select(_ => random.Next()).ToArray();
            var builder = new SortedArrayBuilder<Key, int>(randomArray.Select(v => (new Key(v), v)));
            var blob = builder.CreateManagedBlobAssetReference();
            Assert.That(blob.Value.Length, Is.EqualTo(randomArray.Length));
            foreach (var key in randomArray) Assert.That(blob.Value[key], Is.EqualTo(key), $"key = {key}");
        }
    }

}