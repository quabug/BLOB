using System;
using System.Collections.Generic;

namespace Blob
{
    public struct BlobSortedArray<TKey, TValue>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        public BlobArray<TKey> Keys;
        public BlobArray<TValue> Values;

        public int Length => Values.Length;

        public ref TValue this[TKey key]
        {
            get
            {
                var index = IndexOfKey(key);
                if (index < 0) throw new IndexOutOfRangeException("cannot found value");
                return ref Values[index];
            }
        }

        public int IndexOfKey(TKey key)
        {
            return SearchIndexInRange(0, Keys.Length, key, key.GetHashCode());
        }

        int SearchIndexInRange(int startIndex, int count, TKey key, int keyHashCode)
        {
            if (count <= 0) return -1;
            var index = startIndex + count / 2;
            var thisHashCode = Keys[index].GetHashCode();
            if (keyHashCode == thisHashCode) return SearchIndexInSameHashKeys(index, key, keyHashCode);
            var leftCount = index - startIndex;
            if (keyHashCode < thisHashCode) return SearchIndexInRange(startIndex, leftCount, key, keyHashCode);
            var rightCount = count - leftCount - 1;
            return SearchIndexInRange(index + 1, rightCount, key, keyHashCode);
        }

        int SearchIndexInSameHashKeys(int index, TKey key, int hashCode)
        {
            var equalityComparer = EqualityComparer<TKey>.Default;
            var nextIndex = index;
            while (nextIndex < Keys.Length)
            {
                if (hashCode != Keys[nextIndex].GetHashCode()) break;
                if (equalityComparer.Equals(key, Keys[nextIndex])) return nextIndex;
                nextIndex++;
            }

            nextIndex = index - 1;
            while (nextIndex >= 0)
            {
                if (hashCode != Keys[nextIndex].GetHashCode()) break;
                if (equalityComparer.Equals(key, Keys[nextIndex])) return nextIndex;
                nextIndex--;
            }

            return -1;
        }
    }
}