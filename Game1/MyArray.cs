namespace Game1
{
    // may delete this
    [Serializable]
    public class MyArray<T> : ConstArray<T>
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

        protected MyArray(IEnumerable<T> values)
            : base(values: values)
        { }

        public new T this[ResInd resInd]
        {
            get => array[(int)resInd];
            set => array[(int)resInd] = value;
        }
    }
}
