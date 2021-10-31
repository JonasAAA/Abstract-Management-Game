using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public class ULongArray : ConstULongArray
    {
        public ULongArray()
            : base()
        { }

        public ULongArray(ulong value)
            : base(value: value)
        { }

        public ULongArray(IEnumerable<ulong> values)
            : base(values: values)
        { }

        public new ulong this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }
}
