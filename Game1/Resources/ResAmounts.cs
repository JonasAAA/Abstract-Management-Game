using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmounts : IMyArray<ulong>, IEquatable<ResAmounts>
    {
        private readonly ulong[] array;

        public ResAmounts()
            => array = new ulong[ResInd.count];

        public ResAmounts(ResAmount resAmount)
            : this()
            => this[resAmount.resInd] = resAmount.amount;

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

        public ResAmounts ConvertToBasic()
        {
            ResAmounts result = new();
            foreach (var resInd in ResInd.All)
                if (this[resInd] > 0)
                    result += this[resInd] * CurResConfig.resources[resInd].BasicIngredients;
            return result;

            // The commented out bit should produce the same result, be be a little more efficient
            // If want to use it, may need to update it
            //ulong[] result = new ulong[ResInd.count];
            //for (ulong resInd = 0; resInd < BasicResInd.count; resInd++)
            //    result[resInd] = array[resInd];
            //for (ulong resInd = BasicResInd.count; resInd < ResInd.count; resInd++)
            //    for (ulong otherResInd = 0; otherResInd < ResInd.count; otherResInd++)
            //        result[otherResInd] += CurResConfig.resources[(NonBasicResInd)resInd].ingredients.array[otherResInd];
            //return new(values: result);
        }

        public bool IsEmpty()
            => array.Sum() is 0;

        public ResAmounts Min(ResAmounts resAmounts)
            => new(array.Zip(resAmounts, (a, b) => MyMathHelper.Min(a, b)));

        public ulong TotalMass()
            => CurResConfig.resources.Zip(array).Sum(item => item.First.Mass * item.Second);

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

        public static bool operator ==(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => resAmounts1.array.Zip(resAmounts2.array).All(pair => pair.First == pair.Second);

        public static bool operator !=(ResAmounts resAmounts1, ResAmounts resAmounts2)
            => !(resAmounts1 == resAmounts2);

        public bool Equals(ResAmounts other)
            => this == other;

        public override bool Equals(object? obj)
            => obj is ResAmounts other && Equals(other: other);

        public override int GetHashCode()
            => HashCode.Combine(array);
    }
}
