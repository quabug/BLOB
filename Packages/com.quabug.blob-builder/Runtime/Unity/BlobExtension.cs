#if UNITY_BLOB

using System.IO;
using Unity.Entities;

namespace Blob
{
    public static partial class BlobExtension
    {
        public static BlobAssetReference<T> CreateUnityBlobAssetReference<T>(this IBlobBuilder<T> builder) where T : unmanaged
        {
            using var stream = new MemoryStream();
            builder.CreateBlob(stream, 16);
            var alignedLength = Utilities.Align(stream.Length, 16);
            // expand stream to 16-bytes-aligned length as same as Unity BLOB
            stream.SetLength(alignedLength);
            return BlobAssetReference<T>.Create(stream.ToArray());
        }
    }
}

#endif