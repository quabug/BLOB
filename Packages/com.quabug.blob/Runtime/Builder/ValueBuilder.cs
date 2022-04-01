namespace Blob
{
    public class ValueBuilder<T> : Builder<T> where T : unmanaged
    {
        private T _value;
        public ref T Value => ref _value;

        public ValueBuilder() => _value = default(T);
        public ValueBuilder(T value) => _value = value;

        protected override void BuildImpl(IBlobStream stream)
        {
            stream.WriteValue(ref _value);
        }
    }
}