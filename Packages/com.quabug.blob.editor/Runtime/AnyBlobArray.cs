using System;
using AnySerialize;
using UnityEngine;

namespace Blob
{
    [Serializable]
    public class AnyBlobArray<T, [AnyConstraintType] TAny> : Builder<BlobArray<T>>, IReadOnlyAnyBlob<BlobArray<T>>
        where T : unmanaged
        where TAny : IReadOnlyAnyBlob<T>
    {
        [SerializeField] private TAny[] _value;
        
        protected override void BuildImpl(IBlobStream stream, ref BlobArray<T> data)
        {
            stream.WriteArrayMeta(_value.Length).ToPatchPosition();
            foreach (var v in _value) v.Build(stream);
        }
    }
}