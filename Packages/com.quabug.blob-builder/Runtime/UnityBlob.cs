#if UNITY_BLOB

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Unity.Entities;

namespace Blob
{
    public class UnityBlobPtrBuilder<T> : BlobPtrBuilder<T, Unity.Entities.BlobPtr<T>> where T : unmanaged
    {
        public UnityBlobPtrBuilder() {}
        public UnityBlobPtrBuilder(T value) : base(value) {}
        public UnityBlobPtrBuilder(IBlobBuilder<T> builder) : base(builder) {}
    }

    public class UnityBlobArrayBuilder<T> : BlobArrayBuilder<T, Unity.Entities.BlobArray<T>> where T : unmanaged
    {
        public UnityBlobArrayBuilder() {}
        public UnityBlobArrayBuilder(IEnumerable<T> elements) : base(elements) {}
        public UnityBlobArrayBuilder(IEnumerable<IBlobBuilder<T>> builders) : base(builders) {}
    }

    public class UnityBlobStringBuilder : BlobArrayBuilder<byte, Unity.Entities.BlobString>
    {
        public UnityBlobStringBuilder() {}
        // TODO: optimize?
        public UnityBlobStringBuilder([NotNull] string str) : base(Encoding.UTF8.GetBytes(str).Append((byte)0)) {}
    }

    public static partial class BlobExtension
    {
        public static BlobAssetReference<T> CreateUnityBlobAssetReference<T>(this IBlobBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream, 16);
            var alignedLength = Utilities.Align(stream.Length, 16);
            // expand stream to 16-bytes-aligned length as same as Unity BLOB
            if (stream.Length < alignedLength)
            {
                stream.Seek(alignedLength - 1, SeekOrigin.Begin);
                stream.WriteByte(0);
            }
            return BlobAssetReference<T>.Create(stream.ToArray());
        }
    }
}

#endif