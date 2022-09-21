namespace Blob
{
    public interface IReadOnlyAnyBlob<T> : IBuilder<T> where T : unmanaged {}
}