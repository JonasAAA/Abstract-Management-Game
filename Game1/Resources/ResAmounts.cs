using System.Numerics;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmounts : IFormOfEnergy<ResAmounts>, IMyArray<ulong>, IEquatable<ResAmounts>, IAdditionOperators<ResAmounts, ResAmounts, ResAmounts>, IAdditiveIdentity<ResAmounts, ResAmounts>, IMultiplyOperators<ResAmounts, ulong, ResAmounts>, IMultiplicativeIdentity<ResAmounts, ulong>, IMin<ResAmounts>
    {
        public static ResAmounts Empty
            => emptyResAmounts;

        public static readonly ResAmounts magicUnlimitedResAmounts; 

        static ResAmounts IAdditiveIdentity<ResAmounts, ResAmounts>.AdditiveIdentity
            => Empty;

        static ulong IMultiplicativeIdentity<ResAmounts, ulong>.MultiplicativeIdentity
            => 1;

        private static readonly ResAmounts emptyResAmounts;

        static ResAmounts()
        {
            emptyResAmounts = new();
            Debug.Assert(CurWorldConfig.magicUnlimitedResAmounts.Length == (int)ResInd.count);
            magicUnlimitedResAmounts = new(values: CurWorldConfig.magicUnlimitedResAmounts);
        }

        private readonly ulong[] array;

        bool IFormOfEnergy<ResAmounts>.IsZero
            => Mass().IsZero;

        public ResAmounts()
            => array = new ulong[ResInd.count];

        public ResAmounts(ResAmount resAmount)
            : this(resInd: resAmount.resInd, amount: resAmount.amount)
        { }

        public ResAmounts(ResInd resInd, ulong amount)
            : this()
            => this[resInd] = amount;

        private ResAmounts(IEnumerable<ulong> values)
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
            ResAmounts result = Empty;
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
            => this == emptyResAmounts || array.Sum() is 0;

        public Mass Mass()
            => CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Mass, scalars: array);

        public HeatCapacity HeatCapacity()
            => CurResConfig.resources.CombineLinearly(vectorSelector: res => res.HeatCapacity, scalars: array);

        // analogous to with expression from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression
        public ResAmounts With(ResAmount resAmount)
            => new(this)
            {
                [resAmount.resInd] = resAmount.amount
            };

        public ResAmounts WithAdd(ResAmount resAmount)
            => new(this)
            {
                [resAmount.resInd] = this[resAmount.resInd] + resAmount.amount
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

        public static explicit operator Energy(ResAmounts resAmounts)
            => Energy.CreateFromJoules(valueInJ: resAmounts.Mass().valueInKg * CurWorldConfig.energyInJPerKgOfMass);

        public static ResAmounts operator +(ResAmounts left, ResAmounts right)
            => new(left.Zip(right, (a, b) => a + b));

        public static ResAmounts operator -(ResAmounts left, ResAmounts right)
            => new(left.Zip(right, (a, b) => a - b));

        public static ResAmounts operator *(ulong value, ResAmounts resAmounts)
            => new(from a in resAmounts select value * a);

        public static ResAmounts operator *(ResAmounts resAmounts, ulong value)
            => value * resAmounts;

        public static ResAmounts operator /(ResAmounts resAmounts, ulong value)
            => new(from a in resAmounts select a / value);

        ///// <returns> some elements can be None </returns>
        //public static ConstArray<UDouble> operator /(ResAmounts left, ResAmounts right)
        //    => new(left.Zip(right, (a, b) => (UDouble)a / b));

        static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator >(ResAmounts left, ResAmounts right)
            => left >= right && left != right;

        static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator <(ResAmounts left, ResAmounts right)
            => left <= right && left != right;

        public static bool operator <=(ResAmounts left, ResAmounts right)
            => left.Zip(right).All(a => a.First <= a.Second);

        public static bool operator >=(ResAmounts left, ResAmounts right)
            => left.Zip(right).All(a => a.First >= a.Second);

        public static bool operator ==(ResAmounts left, ResAmounts right)
            => left.array == right.array || left.array.Zip(right.array).All(pair => pair.First == pair.Second);

        public static bool operator !=(ResAmounts left, ResAmounts right)
            => !(left == right);


        public bool Equals(ResAmounts other)
            => this == other;

        public override bool Equals(object? obj)
            => obj is ResAmounts other && Equals(other: other);

        public override int GetHashCode()
            => HashCode.Combine(array);

        static ResAmounts IMin<ResAmounts>.Min(ResAmounts left, ResAmounts right)
            => new(left.array.Zip(right.array, (a, b) => MyMathHelper.Min(a, b)));
    }
}
