namespace Game1
{
    // may delete this
    [Serializable]
    public sealed class MyArray<T> : ConstArray<T>
    {
        public MyArray()
            : base()
        { }

        public MyArray(T value)
            : base(value: value)
        { }

        public MyArray(Func<ResInd, T> selector)
            : base(selector: selector)
        { }

        public new T this[ResInd resInd]
        {
            get => array[(int)resInd];
            set => array[(int)resInd] = value;
        }
    }
}
