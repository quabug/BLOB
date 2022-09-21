using System;
using UnityEngine;

namespace Blob
{
    [Serializable]
    public class AnyBlobValue<T> : Builder<T>, IReadOnlyAnyBlob<T> where T : unmanaged
    {
        [SerializeField] private T _value;

        protected override void BuildImpl(IBlobStream stream, ref T data)
        {
            data = _value;
        }
    }
}