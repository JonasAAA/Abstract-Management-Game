using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class MyArray<T> : IEnumerable<T>
    {
        public static readonly int length;

        static MyArray()
            => length = 3;

        protected readonly T[] array;

        public MyArray()
            => array = new T[length];

        public MyArray(T value)
            : this()
            => Array.Fill(array: array, value: value);

        protected MyArray(IEnumerable<T> values)
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
}
