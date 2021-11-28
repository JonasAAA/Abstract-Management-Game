using System;
using System.Collections.Generic;
using System.Linq;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
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
            => Enumerable.Range(start: 0, count: length).Sum(i => CurResConfig.resources[i].weight * array[i]);

        public override string ToString()
        {
            if (array.All(value => value is 0))
                return "None";
            string result = "";
            for (int resInd = 0; resInd < length; resInd++)
                if (array[resInd] > 0)
                    result += $"res{resInd}: {array[resInd]}, ";
            return result.Trim(' ', ',');
        }

        public static ULongArray operator +(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a + b));

        public static ULongArray operator -(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a - b));

        public static ULongArray operator *(ulong value, ConstULongArray ulongArray)
            => new(from a in ulongArray select value * a);

        public static ULongArray operator *(ConstULongArray ulongArray, ulong value)
            => value * ulongArray;

        public static ULongArray operator /(ConstULongArray ulongArray, ulong value)
            => new(from a in ulongArray select a / value);

        /// <returns> some elements can be None </returns>
        public static MyArray<double> operator /(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => (double)a / b));

        public static bool operator <=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First <= a.Second);

        public static bool operator >=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First >= a.Second);
    }
}
