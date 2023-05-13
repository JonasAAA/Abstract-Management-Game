﻿//using System.Numerics;
//using static Game1.WorldManager;

//namespace Game1.Resources
//{
//    [Serializable]
//    public readonly struct ResAmounts : IFormOfEnergy<ResAmounts>, IMyArray<ulong>, IEquatable<ResAmounts>, IAdditionOperators<ResAmounts, ResAmounts, ResAmounts>,
//        IAdditiveIdentity<ResAmounts, ResAmounts>, IMultiplyOperators<ResAmounts, ulong, ResAmounts>, IMultiplicativeIdentity<ResAmounts, ulong>, IMin<ResAmounts>
//    {
//        public static readonly ResAmounts empty, magicUnlimitedResAmounts; 

//        static ResAmounts IAdditiveIdentity<ResAmounts, ResAmounts>.AdditiveIdentity
//            => empty;

//        static ulong IMultiplicativeIdentity<ResAmounts, ulong>.MultiplicativeIdentity
//            => 1;

//        static ResAmounts()
//        {
//            empty = new();
//            magicUnlimitedResAmounts = new
//            (
//                values: Enumerable.Repeat
//                (
//                    element: (ulong)uint.MaxValue,
//                    count: (int)ResInd.count
//                )
//            );
//        }

//        private readonly ulong[] array;

//        bool IFormOfEnergy<ResAmounts>.IsZero
//            => Mass().IsZero;

//        public ResAmounts()
//            => array = new ulong[ResInd.count];

//        public ResAmounts(ResAmount resAmount)
//            : this(resInd: resAmount.resInd, amount: resAmount.amount)
//        { }

//        public ResAmounts(ResInd resInd, ulong amount)
//            : this()
//            => this[resInd] = amount;

//        private ResAmounts(IEnumerable<ulong> values)
//        {
//            array = values.ToArray();
//            if (array.Length != (int)ResInd.count)
//                throw new ArgumentException();
//        }

//        public ulong this[ResInd resInd]
//        {
//            get => array[(int)resInd];
//            init => array[(int)resInd] = value;
//        }

//        public ResAmounts ConvertToBasic()
//        {
//            ResAmounts result = empty;
//            foreach (var resInd in ResInd.All)
//                if (this[resInd] > 0)
//                    result += this[resInd] * CurResConfig.resources[resInd].BasicIngredients;
//            return result;

//            // The commented out bit should produce the same result, be be a little more efficient
//            // If want to use it, may need to update it
//            //ulong[] result = new ulong[ResInd.count];
//            //for (ulong resInd = 0; resInd < BasicResInd.count; resInd++)
//            //    result[resInd] = array[resInd];
//            //for (ulong resInd = BasicResInd.count; resInd < ResInd.count; resInd++)
//            //    for (ulong otherResInd = 0; otherResInd < ResInd.count; otherResInd++)
//            //        result[otherResInd] += CurResConfig.resources[(NonBasicResInd)resInd].ingredients.array[otherResInd];
//            //return new(values: result);
//        }

//        public bool IsEmpty()
//            => this == empty || array.Sum() is 0;

//        public Mass Mass()
//            => CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Mass, scalars: array);

//        public HeatCapacity HeatCapacity()
//            => CurResConfig.resources.CombineLinearly(vectorSelector: res => res.HeatCapacity, scalars: array);

//        public ulong Area()
//            => CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Area, scalars: array);

//        public Propor Reflectance()
//            => Propor.Create
//            (
//                part: CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Area * res.Reflectance, scalars: array),
//                whole: CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Area, scalars: array)
//            )!.Value;

//        public Propor Emissivity()
//            => Propor.Create
//            (
//                part: CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Area * res.Emissivity, scalars: array),
//                whole: CurResConfig.resources.CombineLinearly(vectorSelector: res => res.Area, scalars: array)
//            )!.Value;

//        // analogous to with expression from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression
//        public ResAmounts With(ResAmount resAmount)
//            => new(this)
//            {
//                [resAmount.resInd] = resAmount.amount
//            };

//        public ResAmounts WithAdd(ResAmount resAmount)
//            => new(this)
//            {
//                [resAmount.resInd] = this[resAmount.resInd] + resAmount.amount
//            };

//        public override string ToString()
//        {
//            if (array.All(value => value is 0))
//                return "None";
//            string result = "";
//            foreach (var resInd in ResInd.All)
//                if (this[resInd] > 0)
//                    result += $"res{resInd}: {this[resInd]}, ";
//            return result.Trim(' ', ',');
//        }

//        public static explicit operator Energy(ResAmounts resAmounts)
//            => Energy.CreateFromJoules(valueInJ: resAmounts.Mass().valueInKg * CurWorldConfig.energyInJPerKgOfMass);

//        public static ResAmounts operator +(ResAmounts left, ResAmounts right)
//            => new(left.Zip(right, (a, b) => a + b));

//        public static ResAmounts operator -(ResAmounts left, ResAmounts right)
//            => new(left.Zip(right, (a, b) => a - b));

//        public static ResAmounts operator *(ulong value, ResAmounts resAmounts)
//            => new(from a in resAmounts select value * a);

//        public static ResAmounts operator *(ResAmounts resAmounts, ulong value)
//            => value * resAmounts;

//        static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator >(ResAmounts left, ResAmounts right)
//            => left >= right && left != right;

//        static bool IComparisonOperators<ResAmounts, ResAmounts, bool>.operator <(ResAmounts left, ResAmounts right)
//            => left <= right && left != right;

//        public static bool operator <=(ResAmounts left, ResAmounts right)
//            => left.Zip(right).All(a => a.First <= a.Second);

//        public static bool operator >=(ResAmounts left, ResAmounts right)
//            => left.Zip(right).All(a => a.First >= a.Second);

//        public static bool operator ==(ResAmounts left, ResAmounts right)
//            => left.array == right.array || left.array.Zip(right.array).All(pair => pair.First == pair.Second);

//        public static bool operator !=(ResAmounts left, ResAmounts right)
//            => !(left == right);


//        public bool Equals(ResAmounts other)
//            => this == other;

//        public override bool Equals(object? obj)
//            => obj is ResAmounts other && Equals(other: other);

//        public override int GetHashCode()
//            => HashCode.Combine(array);

//        static ResAmounts IMin<ResAmounts>.Min(ResAmounts left, ResAmounts right)
//            => new(left.array.Zip(right.array, MyMathHelper.Min));
//    }
//}
