using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class IntArray : MyArray<int>
    {
        public IntArray()
            : base()
        { }

        public IntArray(int value)
            : base(value: value)
        { }

        private IntArray(IEnumerable<int> values)
            : base(values: values)
        { }

        // this = PosPart - NegPart
        // PosPart, NegPart >= 0
        public IntArray PosPart
            => new(array.Select(a => Math.Max(a, 0)));

        public IntArray NegPart
            => new(array.Select(a => Math.Max(-a, 0)));

        public static IntArray operator +(IntArray intArray1, IntArray intArray2)
            => new(intArray1.Zip(intArray2, (a, b) => a + b));

        public static IntArray operator -(IntArray intArray)
            => new(intArray.Select(a => -a));

        public static IntArray operator -(IntArray intArray1, IntArray intArray2)
            => new(intArray1.Zip(intArray2, (a, b) => a - b));

        public static IntArray operator *(int value, IntArray intArray)
            => new(intArray.array.Select(a => value * a));

        public static IntArray operator *(IntArray intArray, int value)
            => value * intArray;

        public static bool operator <=(IntArray intArray1, IntArray intArray2)
            => intArray1.Zip(intArray2, (a, b) => a <= b).All(c => c);

        public static bool operator >=(IntArray intArray1, IntArray intArray2)
            => intArray1.Zip(intArray2, (a, b) => a >= b).All(c => c);
    }
}
