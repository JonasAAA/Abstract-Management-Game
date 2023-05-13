//namespace Game1
//{
//    [Serializable]
//    public class ConstArray<T> : IMyArray<T>
//    {
//        protected readonly T[] array;

//        public ConstArray()
//            => array = new T[ResInd.count];

//        public ConstArray(T value)
//            : this()
//            => Array.Fill(array: array, value: value);

//        public ConstArray(Func<ResInd, T> selector)
//            : this()
//        {
//            foreach (var resInd in ResInd.All)
//                this[resInd] = selector(resInd);
//        }

//        protected ConstArray(IEnumerable<T> values)
//        {
//            array = values.ToArray();
//            if (array.Length != (int)ResInd.count)
//                throw new ArgumentException();
//        }

//        public T this[ResInd resInd]
//        {
//            get => array[(int)resInd];
//            init => array[(int)resInd] = value;
//        }
//    }
//}
