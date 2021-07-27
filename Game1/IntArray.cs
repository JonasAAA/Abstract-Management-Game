using System.Collections.Generic;

namespace Game1
{
    public class IntArray : ConstIntArray
    {
        public IntArray()
            : base()
        { }

        public IntArray(int value)
            : base(value: value)
        { }

        public IntArray(IEnumerable<int> values)
            : base(values: values)
        { }

        public new int this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }
}
