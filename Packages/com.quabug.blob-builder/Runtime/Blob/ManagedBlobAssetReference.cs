namespace Blob
{
    public unsafe class ManagedBlobAssetReference<T> where T : unmanaged
    {
        private byte[] _blob;
        private readonly T* _value;

        public ref T Value => ref *_value;

        internal ManagedBlobAssetReference(byte[] blob)
        {
            _blob = blob;
            fixed (void* ptr = &blob[0]) _value = (T*)ptr;
        }
    }
}