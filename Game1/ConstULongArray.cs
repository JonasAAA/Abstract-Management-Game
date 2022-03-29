using System.Collections;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public readonly struct ConstULongArray : IMyArray, IEnumerable<ulong>
    {
        private readonly ulong[] array;

        public ConstULongArray()
            => array = new ulong[IMyArray.length];

        public ConstULongArray(ulong value)
            : this()
            => Array.Fill(array: array, value: value);

        public ConstULongArray(IEnumerable<ulong> values)
        {
            array = values.ToArray();
            if (array.Length != IMyArray.length)
                throw new ArgumentException();
        }

        public ulong this[int index]
        {
            get => array[index];
            init => array[index] = value;
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            for (int i = 0; i < IMyArray.length; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public bool IsEmpty()
            => array.Sum() is 0;

        public ConstULongArray Min(ConstULongArray ulongArray)
            => new(array.Zip(ulongArray, (a, b) => Math.Min(a, b)));

        public ulong TotalWeight()
            => CurResConfig.resources.Zip(array).Sum(item => item.First.weight * item.Second);
        // TODO: cleanup
            //=> Enumerable.Range(start: 0, count: IMyArray.length).Sum(i => CurResConfig.resources[i].weight * array[i]);

        // analogous to with expression from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression
        public ConstULongArray With(int index, ulong value)
        {
            ConstULongArray result = new(array);
            result.array[index] = value;
            return result;
        }

        public ConstULongArray WithAdd(int index, ulong value)
        {
            ConstULongArray result = new(array);
            result.array[index] += value;
            return result;
        }

        public override string ToString()
        {
            if (array.All(value => value is 0))
                return "None";
            string result = "";
            for (int resInd = 0; resInd < IMyArray.length; resInd++)
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
