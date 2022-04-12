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

        protected override unsafe void BuildImpl(IBlobStream stream)
        {
            stream.EnsureDataSize<BlobPtrAny>()
                .WriteArrayMeta(sizeof(T))
                .ToPatchPosition()
                .WriteValue(_builder)
                .AlignPatch(Utilities.AlignOf<T>())
            ;
        }
    }

    public class AnyPtrBuilder : Builder<BlobPtrAny>
    {
        private IBuilder _builder;
        private int _size = -1;
        private int _alignment = 0;

        public void SetValue<T>(T value) where T : unmanaged
        {
            SetValue(new ValueBuilder<T>(value));
        }

        public unsafe void SetValue<T>(IBuilder<T> valueBuilder) where T : unmanaged
        {
            _builder = valueBuilder;
            _size = sizeof(T);
            _alignment = Utilities.AlignOf<T>();
        }

        public void SetValue(IBuilder valueBuilder)
        {
            _builder = valueBuilder;
            _size = -1;
            _alignment = 0;
        }

        protected override void BuildImpl(IBlobStream stream)
        {
            stream.EnsureDataSize<BlobPtrAny>().WritePatchOffset();
            var sizePosition = stream.DataPosition;
            var patchPosition = stream.PatchPosition;
            stream.ToPatchPosition().WriteValue(_builder);
            stream.DataPosition = sizePosition;
            stream.WriteValue(_size < 0 ? stream.PatchPosition - patchPosition : _size);
            stream.AlignPatch(_alignment <= 0 ? 4 : _alignment);
        }
    }
}