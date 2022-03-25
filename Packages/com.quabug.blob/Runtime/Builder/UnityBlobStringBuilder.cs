﻿using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityBlobStringBuilder : BlobArrayBuilder<byte, UnityBlobString>
    {
        public UnityBlobStringBuilder() : base(new byte[] { 0 }) {}
        public UnityBlobStringBuilder([NotNull] string str) : base(new UTF8Encoding().GetBytes(str).Append((byte)0)) {}
    }
}