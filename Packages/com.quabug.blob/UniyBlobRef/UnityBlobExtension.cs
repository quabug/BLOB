namespace Unity.Entities
{
    public static class UnityBlobExtension
    {
        public static unsafe int GetLength<T>(this BlobAssetReference<T> blob) where T : unmanaged
        {
            return blob.m_data.Header->Length;
        }
    }
}