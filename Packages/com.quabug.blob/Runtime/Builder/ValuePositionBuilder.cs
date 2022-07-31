using System;

namespace Blob
{
    public class ValuePositionBuilder : IBuilder
    {
        public int DataPosition { get; set; }
        public int DataSize { get; set; }
        public int PatchPosition { get; set; }
        public int PatchSize { get; set; }
        
        public void Build(IBlobStream stream)
        {
            // this builder is only made for record DataPosition
            // so no build process here
            throw new NotSupportedException();
        }
    }
    
    public class ValuePositionBuilder<T> : ValuePositionBuilder, IBuilder<T> where T : unmanaged {}
}