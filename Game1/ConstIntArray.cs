using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class ConstIntArray : ConstArray<int>
    {
        public ConstIntArray()
            : base()
        { }

        public ConstIntArray(int value)
            : base(value: value)
        { }

        public ConstIntArray(IEnumerable<int> values)
            : base(values: values)
        { }

        // this = PosPart - NegPart
        // PosPart, NegPart >= 0
        public IntArray PosPart
            => new(from a in array select Math.Max(a, 0));

        public IntArray NegPart
            => new(from a in array select Math.Max(-a, 0));

        public static IntArray operator +(ConstIntArray intArray1, ConstIntArray intArray2)
            => new(intArray1.Zip(intArray2, (a, b) => a + b));

        public static IntArray operator -(ConstIntArray intArray)
            => new(from a in intArray select -a);

        public static IntArray operator -(ConstIntArray intArray1, ConstIntArray intArray2)
            => new(intArray1.Zip(intArray2, (a, b) => a - b));

        public static IntArray operator *(int value, ConstIntArray intArray)
            => new(from a in intArray select value * a);

        public static IntArray operator *(ConstIntArray intArray, int value)
            => value * intArray;

        public static bool operator <=(ConstIntArray intArray1, ConstIntArray intArray2)
            => intArray1.Zip(intArray2, (a, b) => a <= b).All(c => c);

        public static bool operator >=(ConstIntArray intArray1, ConstIntArray intArray2)
            => intArray1.Zip(intArray2, (a, b) => a >= b).All(c => c);
    }
}
