using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class ConstArray<T> : ConstArray, IEnumerable<T>
    {
        protected readonly T[] array;

        public ConstArray()
            => array = new T[Length];

        public ConstArray(T value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstArray(IEnumerable<T> values)
        {
            array = values.ToArray();
            if (array.Length != Length)
                throw new ArgumentException();
        }

        public T this[int index]
        {
            get => array[index];
            init => array[index] = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public class ConstArray
    {
        public static int Length { get; private set; }

        public static void Initialize(int resCount)
            => Length = resCount;
    }
}
