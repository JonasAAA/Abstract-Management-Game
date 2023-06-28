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
    public readonly struct SomeResAmounts<TRes> : IResAmounts<SomeResAmounts<TRes>>, IEnumerable<ResAmount<TRes>>
        where TRes : class, IResource
    {
        public static readonly SomeResAmounts<TRes> empty;

        static SomeResAmounts()
        {
            empty = new(resList: new(), amounts: new());
        }

        public bool IsEmpty
            // as amounts cannot contain value 0, the only way to be empty is to have no elements
            => Count is 0;

        private int Count
            => resList.Count;

        static SomeResAmounts<TRes> IAdditiveIdentity<SomeResAmounts<TRes>, SomeResAmounts<TRes>>.AdditiveIdentity
            => empty;

        static ulong IMultiplicativeIdentity<SomeResAmounts<TRes>, ulong>.MultiplicativeIdentity
            => 1;

        bool IFormOfEnergy<SomeResAmounts<TRes>>.IsZero
            => IsEmpty;

        /// <summary>
        /// Is reused between multiple instances of Dict to generate very slightly less garbage, e.g. in operator *
        /// </summary>
        private readonly List<TRes> resList;
        private readonly List<ulong> amounts;

        /// <summary>
        /// USE SomeResAmounts.empty instead as it avoids unnecessary allocations
        /// </summary>
        public SomeResAmounts()
            : this(resList: new(), amounts: new())
        { }

        public SomeResAmounts(ResAmount<TRes> resAmount)
            : this(res: resAmount.res, amount: resAmount.amount)
        { }

        public SomeResAmounts(TRes res, ulong amount)
            : this(resList: new() { res }, amounts: new() { amount })
        { }

        /// <summary>
        /// Requirements:
        /// * resList and amounts must contain the same number of elements
        /// * resList must be sorted
        /// * resList must not contain duplicate elements
        /// * amounts should not have a 0 value (as that means memory and processing power of dealing with that element are wasted)
        /// </summary>
        private SomeResAmounts(List<TRes> resList, List<ulong> amounts)
        {
            this.resList = resList;
            this.amounts = amounts;
            Validate();
        }

        public SomeResAmounts(Dictionary<TRes, ulong> resAmounts)
            : this(resAmounts: resAmounts.Select(resAmount => new ResAmount<TRes>(res: resAmount.Key, amount: resAmount.Value)))
        { }

        public SomeResAmounts(IEnumerable<ResAmount<TRes>> resAmounts)
            : this(resAmounts: resAmounts.ToList())
        { }

        public SomeResAmounts(List<ResAmount<TRes>> resAmounts)
        {
            resAmounts.Sort(static (left, right) => CurResConfig.CompareRes(left: left.res, right: right.res));
            resList = new(resAmounts.Count);
            amounts = new(resAmounts.Count);
            for (int ind = 0; ind < resAmounts.Count; ind++)
            {
                if (resAmounts[ind].amount is 0)
                    continue;
                resList.Add(resAmounts[ind].res);
                amounts.Add(resAmounts[ind].amount);
            }
            Validate();
        }

        private void Validate()
        {
#if DEBUG
            Debug.Assert(resList.Count == amounts.Count);
            for (int i = 0; i < resList.Count - 1; i++)
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

        public RawMaterialsMix RawMatComposition()
        {
            var rawMatComp = RawMaterialsMix.empty;
            for (int ind = 0; ind < Count; ind++)
                rawMatComp += resList[ind].RawMatComposition * amounts[ind];
            return rawMatComp;
        }

        public SomeResAmounts<IResource> Generalize()
        {
            List<IResource> newResList = new(Count);
            for (int ind = 0; ind < Count; ind++)
                newResList.Add(resList[ind]);
            return new(resList: newResList, amounts: amounts);
        }

        public ulong NumberOfTimesLargerThan(SomeResAmounts<TRes> other)
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

        // May need to change this if later on materials and/or products are able to store energy
        public static explicit operator Energy(SomeResAmounts<TRes> formOfEnergy)
            => Energy.CreateFromJoules(valueInJ: formOfEnergy.Mass().valueInKg * CurWorldConfig.energyInJPerKgOfMass);

        static SomeResAmounts<TRes> IMin<SomeResAmounts<TRes>>.Min(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
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

        private static (TRes? res, ulong amount) GetResAndAmount(SomeResAmounts<TRes> someResAmounts, int ind)
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

        public static SomeResAmounts<TRes> operator +(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
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

        public static SomeResAmounts<TRes> operator -(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
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

        public static SomeResAmounts<TRes> operator *(SomeResAmounts<TRes> left, ulong right)
        {
            if (right is 0)
                return empty;
            return new(left.resList, left.amounts.Select(amount => amount * right).ToList());
        }

        public static SomeResAmounts<TRes> operator *(ulong left, SomeResAmounts<TRes> right)
            => right * left;

        bool IEquatable<SomeResAmounts<TRes>>.Equals(SomeResAmounts<TRes> other)
            => this == other;

        public static bool operator ==(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
        {
            if (left.Count != right.Count)
                return false;
            for (int ind = 0; ind < left.Count; ind++)
                if (left.amounts[ind] != right.amounts[ind])
                    return false;
            return true;
        }

        public static bool operator !=(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
            => !(left == right);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is SomeResAmounts<TRes> someResAmounts && this == someResAmounts;

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

        public static bool operator >=(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
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

        public static bool operator <=(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
            => right >= left;

        static bool IComparisonOperators<SomeResAmounts<TRes>, SomeResAmounts<TRes>, bool>.operator >(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
            => left >= right && left != right;

        static bool IComparisonOperators<SomeResAmounts<TRes>, SomeResAmounts<TRes>, bool>.operator <(SomeResAmounts<TRes> left, SomeResAmounts<TRes> right)
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
