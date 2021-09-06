﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class ConstULongArray : ConstArray<ulong>
    {
        public ConstULongArray()
            : base()
        { }

        public ConstULongArray(ulong value)
            : base(value: value)
        { }

        public ConstULongArray(IEnumerable<ulong> values)
            : base(values: values)
        { }

        public bool IsEmpty()
            => array.Sum() is 0;

        public ULongArray Min(ConstULongArray ulongArray)
            => new(array.Zip(ulongArray, (a, b) => Math.Min(a, b)));

        public ULongArray ToULongArray()
            => new(array);

        public ulong TotalWeight()
            => Enumerable.Range(start: 0, count: (int)length).Sum(i => Resource.all[i].weight * array[i]);

        public static ULongArray operator +(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a + b));

        public static ULongArray operator -(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a - b));

        public static ULongArray operator *(ulong value, ConstULongArray ulongArray)
            => new(from a in ulongArray select value * a);

        public static ULongArray operator *(ConstULongArray ulongArray, ulong value)
            => value * ulongArray;

        /// <returns> some elements can be None </returns>
        public static MyArray<double> operator /(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => (double)a / b));

        public static bool operator <=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First <= a.Second);

        public static bool operator >=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First >= a.Second);
    }
}