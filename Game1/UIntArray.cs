using System.Collections.Generic;

namespace Game1
{
    public class UIntArray : ConstUIntArray
    {
        public UIntArray()
            : base()
        { }

        public UIntArray(uint value)
            : base(value: value)
        { }

        public UIntArray(IEnumerable<uint> values)
            : base(values: values)
        { }

        public new uint this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }
}
