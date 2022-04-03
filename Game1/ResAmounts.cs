using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public readonly struct ResAmounts : IMyArray<ulong>
    {
        private readonly ulong[] array;

        public ResAmounts()
            => array = new ulong[ResInd.ResCount];

        public ResAmounts(ulong value)
            : this()
            => Array.Fill(array: array, value: value);

        public ResAmounts(IEnumerable<ulong> values)
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

        public ResAmounts Min(ResAmounts resAmounts)
            => new(array.Zip(resAmounts, (a, b) => MathHelper.Min(a, b)));

        public ulong TotalWeight()
            => CurResConfig.resources.Zip(array).Sum(item => item.First.weight * item.Second);

        // analogous to with expression from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression
        public ResAmounts With(ResInd index, ulong value)
            => new(this)
            {
                [index] = value
            };

        public ResAmounts WithAdd(ResInd index, ulong value)
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

        public static ResAmounts operator +(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => new(resAmounts1.Zip(resAmounts2, (a, b) => a + b));

        public static ResAmounts operator -(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => new(resAmounts1.Zip(resAmounts2, (a, b) => a - b));

        public static ResAmounts operator *(ulong value, ResAmounts resAmounts)
            => new(from a in resAmounts select value * a);

        public static ResAmounts operator *(ResAmounts resAmounts, ulong value)
            => value * resAmounts;

        public static ResAmounts operator /(ResAmounts resAmounts, ulong value)
            => new(from a in resAmounts select a / value);

        /// <returns> some elements can be None </returns>
        public static ConstArray<double> operator /(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => new(resAmounts1.Zip(resAmounts2, (a, b) => (double)a / b));

        public static bool operator <=(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => resAmounts1.Zip(resAmounts2).All(a => a.First <= a.Second);

        public static bool operator >=(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => resAmounts1.Zip(resAmounts2).All(a => a.First >= a.Second);
    }
}
