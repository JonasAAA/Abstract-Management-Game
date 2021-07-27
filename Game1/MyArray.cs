using System.Collections.Generic;

namespace Game1
{
    public class MyArray<T> : ConstArray<T>
    {
        public MyArray()
            : base()
        { }

        public MyArray(T value)
            : base(value: value)
        { }

        public MyArray(IEnumerable<T> values)
            : base(values: values)
        { }

        public new T this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }
}
