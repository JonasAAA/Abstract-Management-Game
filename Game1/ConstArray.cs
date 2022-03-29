namespace Game1
{
    [Serializable]
    public class ConstArray<T> : IMyArray<T>
    {
        protected readonly T[] array;

        public ConstArray()
            => array = new T[ResInd.ResCount];

        public ConstArray(T value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstArray(IEnumerable<T> values)
        {
            array = values.ToArray();
            if (array.Length != ResInd.ResCount)
                throw new ArgumentException();
        }

        public T this[ResInd resInd]
        {
            get => array[(int)resInd];
            init => array[(int)resInd] = value;
        }
    }
}
