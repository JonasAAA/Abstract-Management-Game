using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmounts : IMyArray<ulong>
    {
        private readonly ulong[] array;

        public ResAmounts()
            => array = new ulong[ResInd.count];

        public ResAmounts(ulong value)
            : this()
            => Array.Fill(array: array, value: value);

        public ResAmounts(IEnumerable<ulong> values)
        {
            array = values.ToArray();
            if (array.Length != (int)ResInd.count)
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
            => new(array.Zip(resAmounts, (a, b) => MyMathHelper.Min(a, b)));

        public ulong TotalWeight()
            => CurResConfig.resources.Zip(array).Sum(item => item.First.mass * item.Second);

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
            foreach (var resInd in ResInd.All)
                if (this[resInd] > 0)
                    result += $"res{resInd}: {this[resInd]}, ";
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

        ///// <returns> some elements can be None </returns>
        //public static ConstArray<UDouble> operator /(ResAmounts resAmounts1, ResAmounts resAmounts2)
        //    => new(resAmounts1.Zip(resAmounts2, (a, b) => (UDouble)a / b));

        public static bool operator <=(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => resAmounts1.Zip(resAmounts2).All(a => a.First <= a.Second);

        public static bool operator >=(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => resAmounts1.Zip(resAmounts2).All(a => a.First >= a.Second);
    }
}
