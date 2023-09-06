using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static Game1.WorldManager;

namespace Game1.Collections
{
    /// <summary>
    /// Dictionary with efficient internal storage and O(log N) lookup
    /// </summary>
    [Serializable]
    public readonly struct ResAmounts<TRes> : IResAmounts<ResAmounts<TRes>>, IEnumerable<ResAmount<TRes>>
        where TRes : class, IResource
    {
        public static readonly ResAmounts<TRes> empty;

        static ResAmounts()
            => empty = new();

        public bool IsEmpty
            // as amounts cannot contain value 0, the only way to be empty is to have no elements
            => Count is 0;

        public int Count
            => resList.Count;

        static ResAmounts<TRes> IAdditiveIdentity<ResAmounts<TRes>, ResAmounts<TRes>>.AdditiveIdentity
            => empty;

        static ulong IMultiplicativeIdentity<ResAmounts<TRes>, ulong>.MultiplicativeIdentity
            => 1;

        bool IFormOfEnergy<ResAmounts<TRes>>.IsZero
            => IsEmpty;

        // Is reused between multiple instances of Dict to generate very slightly less garbage, e.g. in operator *
        public readonly EfficientReadOnlyCollection<TRes> resList;
        private readonly EfficientReadOnlyCollection<ulong> amounts;

        /// <summary>
        /// Equivalent to ResAmounts.empty
        /// </summary>
        public ResAmounts()
            : this(resList: EfficientReadOnlyCollection<TRes>.empty, amounts: EfficientReadOnlyCollection<ulong>.empty)
        { }

        public ResAmounts(ResAmount<TRes> resAmount)
            : this(res: resAmount.res, amount: resAmount.amount)
        { }

        public ResAmounts(TRes res, ulong amount)
        {
            if (amount is 0)
            {
                resList = EfficientReadOnlyCollection<TRes>.empty;
                amounts = EfficientReadOnlyCollection<ulong>.empty;
            }
            else
            {
                resList = new List<TRes>() { res }.ToEfficientReadOnlyCollection();
                amounts = new List<ulong>() { amount }.ToEfficientReadOnlyCollection();
            }
            Validate();
        }

        /// <summary>
        /// Requirements:
        /// * resList and amounts must contain the same number of elements
        /// * resList must be sorted
        /// * resList must not contain duplicate elements
        /// * amounts should not have a 0 value (as that means memory and processing power of dealing with that element are wasted)
        /// </summary>
        private ResAmounts(List<TRes> resList, List<ulong> amounts)
            : this(resList: new EfficientReadOnlyCollection<TRes>(resList), amounts: new EfficientReadOnlyCollection<ulong>(amounts))
        { }

        private ResAmounts(EfficientReadOnlyCollection<TRes> resList, EfficientReadOnlyCollection<ulong> amounts)
        {
            this.resList = resList;
            this.amounts = amounts;
            Validate();
        }

        public ResAmounts(Dictionary<TRes, ulong> resAmounts)
            : this(resAmounts: resAmounts.Select(resAmount => new ResAmount<TRes>(res: resAmount.Key, amount: resAmount.Value)))
        { }

        public ResAmounts(IEnumerable<ResAmount<TRes>> resAmounts)
            : this(resAmounts: resAmounts.ToList())
        { }

        public ResAmounts(List<ResAmount<TRes>> resAmounts)
        {
            resAmounts.Sort(static (left, right) => CurResConfig.CompareRes(left: left.res, right: right.res));
            List<TRes> mutableResList = new(resAmounts.Count);
            List<ulong> mutableAmounts = new(resAmounts.Count);
            for (int ind = 0; ind < resAmounts.Count; ind++)
            {
                var res = resAmounts[ind].res;
                var amount = resAmounts[ind].amount;
                if (amount is 0)
                    continue;
                if (mutableResList.Count > 0 && mutableResList[^1] == res)
                    mutableAmounts[^1] += amount;
                else
                {
                    mutableResList.Add(res);
                    mutableAmounts.Add(amount);
                }
            }
            resList = new(mutableResList);
            amounts = new(mutableAmounts);
            Validate();
        }

        private void Validate()
        {
#if DEBUG
            Debug.Assert(resList.Count == amounts.Count);
            for (int i = 1; i < resList.Count; i++)
                Debug.Assert(CurResConfig.CompareRes(left: resList[i - 1], right: resList[i]) < 0);
            Debug.Assert(amounts.All(amount => amount is not 0));
#endif
        }

        public ulong this[TRes res]
        {
            get
            {
                int ind = resList.BinarySearch(res);
                if (ind >= 0)
                    return amounts[ind];
                else
                    return 0;
            }
        }

        public Mass Mass()
        {
            Mass mass = Resources.Mass.zero;
            for (int ind = 0; ind < Count; ind++)
                mass += resList[ind].Mass * amounts[ind];
            return mass;
        }

        public HeatCapacity HeatCapacity()
        {
            HeatCapacity heatCapacity = Resources.HeatCapacity.zero;
            for (int ind = 0; ind < Count; ind++)
                heatCapacity += resList[ind].HeatCapacity * amounts[ind];
            return heatCapacity;
        }

        public AreaInt UsefulArea()
        {
            AreaInt usefulArea = AreaInt.zero;
            for (int ind = 0; ind < Count; ind++)
                usefulArea += resList[ind].UsefulArea * amounts[ind];
            return usefulArea;
        }

        public ResRecipe TurningIntoRawMatsRecipe()
            => ResRecipe.CreateOrThrow(ingredients: ToAll(), results: RawMatComposition().ToAll());

        public RawMatAmounts RawMatComposition()
        {
            var rawMatComp = RawMatAmounts.empty;
            for (int ind = 0; ind < Count; ind++)
                rawMatComp += resList[ind].RawMatComposition * amounts[ind];
            return rawMatComp;
        }

        public ResAmounts<TFilterRes> Filter<TFilterRes>()
            where TFilterRes : class, IResource
        {
            List<TFilterRes> newResList = new(Count);
            List<ulong> newAmounts = new(Count);
            for (int ind = 0; ind < Count; ind++)
                if (resList[ind] is TFilterRes filterRes)
                {
                    newResList.Add(filterRes);
                    newAmounts.Add(amounts[ind]);
                }
            return new(resList: newResList, amounts: newAmounts);
        }

        public AllResAmounts ToAll()
        {
            List<IResource> newResList = new(Count);
            for (int ind = 0; ind < Count; ind++)
                newResList.Add(resList[ind]);
            return new(resList: newResList.ToEfficientReadOnlyCollection(), amounts: amounts);
        }

        public ulong NumberOfTimesLargerThan(ResAmounts<TRes> other)
        {
            ulong numberOfTimesLarger = ulong.MaxValue;
            int thisInd = 0, rightInd = 0;
            while (true)
            {
                (TRes? thisRes, ulong thisAmount) = GetResAndAmount(someResAmounts: this, ind: thisInd);
                (TRes? otherRes, ulong otherAmount) = GetResAndAmount(someResAmounts: other, ind: rightInd);
                int compare = CompareRes(thisRes, otherRes);
                if (compare < 0)
                {
                    // this means this has some, while other has none of this resource
                    thisInd++;
                    continue;
                }
                if (compare > 0)
                {
                    // this means this has node, while other has some of this resource
                    // so this doesn't have more resources than other
                    Debug.Assert(!(this >= other));
                    return 0;
                }
                if (thisRes is null)
                {
                    Debug.Assert(otherRes is null);
                    break;
                }
                if (otherAmount is not 0)
                    numberOfTimesLarger = MyMathHelper.Min(numberOfTimesLarger, thisAmount / otherAmount);
                thisInd++;
                rightInd++;
            }
            Debug.Assert(this >= numberOfTimesLarger * other);
            Debug.Assert(!(this - numberOfTimesLarger * other >= other));
            return numberOfTimesLarger;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "None";
            string result = "\n";
            foreach (var (res, amount) in this)
                result += $"{res}: {amount}\n";
            return result;
        }

        // May need to change this if later on materials and/or products are able to store energy
        public static explicit operator Energy(ResAmounts<TRes> formOfEnergy)
            => Energy.CreateFromJoules(valueInJ: formOfEnergy.Mass().valueInKg * CurWorldConfig.energyInJPerKgOfMass);

        static ResAmounts<TRes> IMin<ResAmounts<TRes>>.Min(ResAmounts<TRes> left, ResAmounts<TRes> right)
        {
            int resultCapacity = MyMathHelper.Min(left.Count, right.Count);
            List<TRes> minResList = new(capacity: resultCapacity);
            List<ulong> minAmounts = new(capacity: resultCapacity);
            int leftInd = 0, rightInd = 0;
            while (true)
            {
                (TRes? leftRes, ulong leftAmount) = GetResAndAmount(someResAmounts: left, ind: leftInd);
                (TRes? rightRes, ulong rightAmount) = GetResAndAmount(someResAmounts: right, ind: rightInd);
                int compare = CompareRes(leftRes, rightRes);
                if (compare < 0)
                {
                    // this means minAmount would be 0, so no need to add it to results
                    leftInd++;
                    continue;
                }
                if (compare > 0)
                {
                    // this means minAmount would be 0, so no need to add it to results
                    rightInd++;
                    continue;
                }
                if (leftRes is null)
                {
                    Debug.Assert(rightRes is null);
                    break;
                }
                minResList.Add(leftRes);
                minAmounts.Add(MyMathHelper.Min(leftAmount, rightAmount));
                leftInd++;
                rightInd++;
            }
            Debug.Assert(minResList.Count <= resultCapacity);
            return new(minResList, minAmounts);
        }

        private static (TRes? res, ulong amount) GetResAndAmount(ResAmounts<TRes> someResAmounts, int ind)
            => (ind < someResAmounts.Count) switch
            {
                true => (res: someResAmounts.resList[ind], amount: someResAmounts.amounts[ind]),
                false => (res: null, amount: 0)
            };

        private static int CompareRes(TRes? left, TRes? right)
            => (left, right) switch
            {
                (not null, not null) => CurResConfig.CompareRes(left: left, right: right),
                (not null, null) => -1,
                (null, not null) => 1,
                (null, null) => 0
            };

        public static ResAmounts<TRes> operator +(ResAmounts<TRes> left, ResAmounts<TRes> right)
        {
            List<TRes> sumResList = new(capacity: left.Count + right.Count);
            List<ulong> sumAmountsList = new(capacity: left.Count + right.Count);
            int leftInd = 0, rightInd = 0;
            while (true)
            {
                (TRes? leftRes, ulong leftAmount) = GetResAndAmount(someResAmounts: left, ind: leftInd);
                (TRes? rightRes, ulong rightAmount) = GetResAndAmount(someResAmounts: right, ind: rightInd);
                int compare = CompareRes(leftRes, rightRes);
                TRes? newRes = null;
                ulong newAmount = 0;
                if (compare <= 0)
                {
                    newRes = leftRes;
                    newAmount += leftAmount;
                    leftInd++;
                }
                if (compare >= 0)
                {
                    newRes = rightRes;
                    newAmount += rightAmount;
                    rightInd++;
                }
                if (newRes is null)
                    break;
                sumResList.Add(newRes);
                sumAmountsList.Add(newAmount);
            }
            return new(sumResList, sumAmountsList);

            // CAN use Span as below to avoid allocating too much memory, then resList and amounts would need to be Arrays to avoid copying

            //// This indirection is necessary as can't put managed pointers on stack
            //Span<(bool isLeft, int ind)> sumResSpan = stackalloc (bool isLeft, int ind)[left.Count + right.Count];
            //Span<ulong> sumAmountsSpan = stackalloc ulong[left.Count + right.Count];
            //int sumInd = 0, thisInd = 0, rightInd = 0;
            //while (true)
            //{
            //    (TRes? thisRes, ulong thisAmount) = GetResAndAmount(someResAmounts: left, ind: thisInd);
            //    (TRes? otherRes, ulong otherAmount) = GetResAndAmount(someResAmounts: right, ind: rightInd);
            //    int compare = CompareRes(thisRes, otherRes);
            //    if (thisRes is null && otherRes is null)
            //        break;
            //    if (compare <= 0)
            //    {
            //        sumResSpan[sumInd] = (true, thisInd);
            //        sumAmountsSpan[sumInd] += thisAmount;
            //        thisInd++;
            //    }
            //    if (compare >= 0)
            //    {
            //        sumResSpan[sumInd] = (false, rightInd);
            //        sumAmountsSpan[sumInd] += otherAmount;
            //        rightInd++;
            //    }
            //    sumInd++;
            //}
            //List<TRes> sumResList = new(sumInd);
            //List<ulong> sumAmounts = new(sumInd);
            //for (int ind = 0; ind < sumResList.Count; ind++)
            //{
            //    sumResList.Add(sumResSpan[ind].isLeft ? left.resList[sumResSpan[ind].ind] : right.resList[sumResSpan[ind].ind]);
            //    sumAmounts.Add(sumAmountsSpan[ind]);
            //}
            //return new(sumResList, sumAmounts);
        }

        public static ResAmounts<TRes> operator -(ResAmounts<TRes> left, ResAmounts<TRes> right)
        {
            List<TRes> diffResList = new(capacity: left.Count + right.Count);
            List<ulong> diffAmounts = new(capacity: left.Count + right.Count);
            int leftInd = 0, rightInd = 0;
            while (true)
            {
                (TRes? leftRes, ulong leftAmount) = GetResAndAmount(someResAmounts: left, ind: leftInd);
                (TRes? rightRes, ulong rightAmount) = GetResAndAmount(someResAmounts: right, ind: rightInd);
                int compare = CompareRes(leftRes, rightRes);
                TRes? newRes = null;
                ulong newAmount = 0;
                if (compare <= 0)
                {
                    newRes = leftRes;
                    newAmount += leftAmount;
                    leftInd++;
                }
                if (compare >= 0)
                {
                    newRes = rightRes;
                    newAmount -= rightAmount;
                    rightInd++;
                }
                if (newRes is null)
                    break;
                // THIS is an important difference from add method version
                if (newAmount is not 0)
                {
                    diffResList.Add(newRes);
                    diffAmounts.Add(newAmount);
                }
            }
            return new(diffResList, diffAmounts);
        }

        public static ResAmounts<TRes> operator *(ResAmounts<TRes> left, ulong right)
        {
            if (right is 0)
                return empty;
            return new(left.resList, left.amounts.Select(amount => amount * right).ToEfficientReadOnlyCollection());
        }

        public static ResAmounts<TRes> operator *(ulong left, ResAmounts<TRes> right)
            => right * left;

        bool IEquatable<ResAmounts<TRes>>.Equals(ResAmounts<TRes> other)
            => this == other;

        public static bool operator ==(ResAmounts<TRes> left, ResAmounts<TRes> right)
        {
            if (left.Count != right.Count)
                return false;
            for (int ind = 0; ind < left.Count; ind++)
                if (left.amounts[ind] != right.amounts[ind])
                    return false;
            return true;
        }

        public static bool operator !=(ResAmounts<TRes> left, ResAmounts<TRes> right)
            => !(left == right);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is ResAmounts<TRes> someResAmounts && this == someResAmounts;

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            for (int ind = 0; ind < Count; ind++)
            {
                hashCode.Add(resList[ind]);
                hashCode.Add(amounts[ind]);
            }
            return hashCode.ToHashCode();
        }

        public static bool operator >=(ResAmounts<TRes> left, ResAmounts<TRes> right)
        {
            if (left.Count < right.Count)
                // since 0 amounts are disallowed, left containing fewer elements means that
                // for some resource the amount is 0 on the left and bigger than 0 on the right
                return false;
            int leftInd = 0, rightInd = 0;
            while (true)
            {
                (TRes? leftRes, ulong leftAmount) = GetResAndAmount(someResAmounts: left, ind: leftInd);
                (TRes? rightRes, ulong rightAmount) = GetResAndAmount(someResAmounts: right, ind: rightInd);
                int compare = CompareRes(leftRes, rightRes);
                if (leftRes is null && rightRes is null)
                    break;
                leftInd++;
                // left res smaller than right res
                if (compare < 0)
                    continue;
                if (compare > 0 || leftAmount < rightAmount)
                    return false;
                rightInd++;
            }
            return true;
        }

        public static bool operator <=(ResAmounts<TRes> left, ResAmounts<TRes> right)
            => right >= left;

        static bool IComparisonOperators<ResAmounts<TRes>, ResAmounts<TRes>, bool>.operator >(ResAmounts<TRes> left, ResAmounts<TRes> right)
            => left >= right && left != right;

        static bool IComparisonOperators<ResAmounts<TRes>, ResAmounts<TRes>, bool>.operator <(ResAmounts<TRes> left, ResAmounts<TRes> right)
            => left <= right && left != right;

        public IEnumerator<ResAmount<TRes>> GetEnumerator()
        {
            for (int ind = 0; ind < Count; ind++)
                yield return new(res: resList[ind], amount: amounts[ind]);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
