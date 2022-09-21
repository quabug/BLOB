using AnySerialize;
using Blob;
using UnityEngine;

public struct BlobData {}

public class BlobEditor : MonoBehaviour
{
    [AnySerialize]
    public ManagedBlobAssetReference<BlobData> Value { get; }
}
