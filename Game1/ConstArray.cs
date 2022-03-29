using System.Collections;

namespace Game1
{
    [Serializable]
    public class ConstArray<T> : IMyArray, IEnumerable<T>
    {
        protected readonly T[] array;

        public ConstArray()
            => array = new T[IMyArray.length];

        public ConstArray(T value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstArray(IEnumerable<T> values)
        {
            array = values.ToArray();
            if (array.Length != IMyArray.length)
                throw new ArgumentException();
        }

        public T this[int index]
        {
            get => array[index];
            init => array[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < IMyArray.length; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
