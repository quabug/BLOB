namespace Blob
{
    public class AnyPtrBuilder<T> : Builder<BlobPtrAny> where T : unmanaged
    {
        private IBuilder<T> _builder;

        public AnyPtrBuilder() => _builder = new ValueBuilder<T>();
        public AnyPtrBuilder(T value) => _builder = new ValueBuilder<T>(value);
        public AnyPtrBuilder(IBuilder<T> builder) => _builder = builder;

        public void SetValue(T value)
        {
            SetValue(new ValueBuilder<T>(value));
        }

        public void SetValue(IBuilder<T> valueBuilder)
        {
            _builder = valueBuilder;
        }

        protected override void BuildImpl(IBlobStream stream, ref BlobPtrAny data)
        {
            data.Data.Offset = stream.PatchOffset() - data.GetFieldOffset(ref data.Data.Offset);
            stream.ToPatchPosition().WriteValue(_builder);
            data.Data.Length = stream.PatchPosition - PatchPosition;
        }
    }

    public class AnyPtrBuilder : Builder<BlobPtrAny>
    {
        private IBuilder _builder;

        public void SetValue<T>(T value) where T : unmanaged
        {
            SetValue(new ValueBuilder<T>(value));
        }

        public void SetValue<T>(IBuilder<T> valueBuilder) where T : unmanaged
        {
            _builder = valueBuilder;
        }

        public void SetValue(IBuilder valueBuilder)
        {
            _builder = valueBuilder;
        }

        protected override void BuildImpl(IBlobStream stream, ref BlobPtrAny data)
        {
            data.Data.Offset = stream.PatchOffset() - data.GetFieldOffset(ref data.Data.Offset);
            stream.ToPatchPosition().WriteValue(_builder);
            data.Data.Length = stream.PatchPosition - PatchPosition;
        }
    }
}