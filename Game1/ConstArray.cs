using System.Collections;

namespace Game1
{
    [Serializable]
    public class ConstArray<T> : ConstArray, IEnumerable<T>
    {
        protected readonly T[] array;

        public ConstArray()
            => array = new T[length];

        public ConstArray(T value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstArray(IEnumerable<T> values)
        {
            array = values.ToArray();
            if (array.Length != length)
                throw new ArgumentException();
        }

        public T this[int index]
        {
            get => array[index];
            init => array[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < length; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    [Serializable]
    public class ConstArray
    {
        public const int length = (int)WorldManager.MaxRes + 1;
    }
}
