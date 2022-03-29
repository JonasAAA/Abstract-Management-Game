using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public readonly struct ConstULongArray : IMyArray<ulong>
    {
        private readonly ulong[] array;

        public ConstULongArray()
            => array = new ulong[ResInd.ResCount];

        public ConstULongArray(ulong value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstULongArray(IEnumerable<ulong> values)
        {
            array = values.ToArray();
            if (array.Length != ResInd.ResCount)
                throw new ArgumentException();
        }

        public ulong this[ResInd resInd]
        {
            get => array[(int)resInd];
            init => array[(int)resInd] = value;
        }

        public bool IsEmpty()
            => array.Sum() is 0;

        public ConstULongArray Min(ConstULongArray ulongArray)
            => new(array.Zip(ulongArray, (a, b) => Math.Min(a, b)));

        public ulong TotalWeight()
            => CurResConfig.resources.Zip(array).Sum(item => item.First.weight * item.Second);
        // TODO: cleanup
            //=> Enumerable.Range(start: 0, count: IMyArray.length).Sum(i => CurResConfig.resources[i].weight * array[i]);

        // analogous to with expression from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression
        public ConstULongArray With(ResInd index, ulong value)
            => new(this)
            {
                [index] = value
            };

        public ConstULongArray WithAdd(ResInd index, ulong value)
            => new(this)
            {
                [index] = this[index] + value
            };

        public override string ToString()
        {
            if (array.All(value => value is 0))
                return "None";
            string result = "";
            for (int resInd = 0; resInd < ResInd.ResCount; resInd++)
                if (array[resInd] > 0)
                    result += $"res{resInd}: {array[resInd]}, ";
            return result.Trim(' ', ',');
        }

        public static ConstULongArray operator +(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a + b));

        public static ConstULongArray operator -(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => a - b));

        public static ConstULongArray operator *(ulong value, ConstULongArray ulongArray)
            => new(from a in ulongArray select value * a);

        public static ConstULongArray operator *(ConstULongArray ulongArray, ulong value)
            => value * ulongArray;

        public static ConstULongArray operator /(ConstULongArray ulongArray, ulong value)
            => new(from a in ulongArray select a / value);

        /// <returns> some elements can be None </returns>
        public static ConstArray<double> operator /(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => new(ulongArray1.Zip(ulongArray2, (a, b) => (double)a / b));

        public static bool operator <=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First <= a.Second);

        public static bool operator >=(ConstULongArray ulongArray1, ConstULongArray ulongArray2)
            => ulongArray1.Zip(ulongArray2).All(a => a.First >= a.Second);
    }
}
