using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class ConstUIntArray : ConstArray<uint>
    {
        public ConstUIntArray()
            : base()
        { }

        public ConstUIntArray(uint value)
            : base(value: value)
        { }

        public ConstUIntArray(IEnumerable<uint> values)
            : base(values: values)
        { }

        public bool IsEmpty
            => array.Sum() is 0;

        public static UIntArray operator +(ConstUIntArray uintArray1, ConstUIntArray uintArray2)
            => new(uintArray1.Zip(uintArray2, (a, b) => a + b));

        //public static NonnegIntArray operator -(ConstNonnegIntArray intArray)
        //    => new(from a in intArray select -a);

        public static UIntArray operator -(ConstUIntArray uintArray1, ConstUIntArray uintArray2)
            => new(uintArray1.Zip(uintArray2, (a, b) => a - b));

        public static UIntArray operator *(uint value, ConstUIntArray uintArray)
            => new(from a in uintArray select value * a);

        public static UIntArray operator *(ConstUIntArray uintArray, uint value)
            => value * uintArray;

        public static bool operator <=(ConstUIntArray uintArray1, ConstUIntArray uintArray2)
            => uintArray1.Zip(uintArray2).All(a => a.First <= a.Second);

        public static bool operator >=(ConstUIntArray uintArray1, ConstUIntArray uintArray2)
            => uintArray1.Zip(uintArray2).All(a => a.First >= a.Second);
    }
}
