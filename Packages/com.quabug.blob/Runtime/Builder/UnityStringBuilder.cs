﻿using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Blob
{
    public class UnityStringBuilder : ArrayBuilder<byte, UnityBlobString>
    {
        public UnityStringBuilder() : base(new byte[] { 0 }) => PatchAlignment = 4;
        public UnityStringBuilder([NotNull] string str) : base(new UTF8Encoding().GetBytes(str).Append((byte)0).ToArray()) => PatchAlignment = 4;
    }
}