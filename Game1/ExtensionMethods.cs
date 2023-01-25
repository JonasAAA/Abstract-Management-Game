using Game1.Inhabitants;
using System.Numerics;
using System.Text;

namespace Game1
{
    public static class ExtensionMethods
    {
        public static void TransferTo<TSourcePile, TDestinPile, TAmount>(this TSourcePile source, TDestinPile destin, TAmount amount)
            where TSourcePile : ISourcePile<TAmount>
            where TDestinPile : IDestinPile<TAmount>
            where TAmount : struct, ICountable<TAmount>
        {
            Pile<TAmount> middleDestin = Pile<TAmount>.CreateEmpty(locationCounters: source.LocationCounters);
            source.TransferTo(destin: middleDestin, amount: amount);
            destin.TransferFrom(source: middleDestin, amount: amount);
        }

        private class EnergyPile<TAmount> : Pile<TAmount>
            where TAmount : struct, IFormOfEnergy<TAmount>
        {
            public new static EnergyPile<TAmount> CreateEmpty(LocationCounters locationCounters)
                => new(locationCounters: locationCounters, counter: EnergyCounter<TAmount>.CreateEmpty());

            protected override EnergyCounter<TAmount> Counter { get; }

            private EnergyPile(LocationCounters locationCounters, EnergyCounter<TAmount> counter)
                : base(locationCounters: locationCounters, counter: counter)
            {
                Counter = counter;
            }

            public void TransformAllTo<TDestinAmount>(EnergyPile<TDestinAmount> destin)
                where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount> 
            {
                TAmount amountToTransform = Amount;
                Counter.TransformTo(destin: destin.Counter, sourceCount: amountToTransform);
                destin.LocationCounters.TransformFrom<TAmount, TDestinAmount>(source: LocationCounters, sourceAmount: amountToTransform);
            }
        }

        public static void TransformAllTo<TSourceAmount, TDestinAmount>(this ISourcePile<TSourceAmount> source, IDestinPile<TDestinAmount> destin)
            //where TSourcePile : ISourcePile<TSourceAmount>
            //where TDestinPile : IDestinPile<TDestinAmount>
            where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            var energySource = EnergyPile<TSourceAmount>.CreateEmpty(locationCounters: source.LocationCounters);
            energySource.TransferAllFrom(source: source);
            var energyDestin = EnergyPile<TDestinAmount>.CreateEmpty(locationCounters: destin.LocationCounters);
            energySource.TransformAllTo(destin: energyDestin);
            destin.TransferAllFrom(source: energyDestin);
        }

        public static ulong ValueInJ<T>(this T formOfEnergy)
            where T : IFormOfEnergy<T>
            => ((Energy)formOfEnergy).valueInJ;

        // could be optimized a la https://stackoverflow.com/questions/11030109/aggregate-vs-sum-performance-in-linq
        public static TVector CombineLinearly<TSource, TVector, TScalar>(this IEnumerable<TSource> source, Func<TSource, TVector> vectorSelector, IEnumerable<TScalar> scalars)
            where TVector : IAdditionOperators<TVector, TVector, TVector>, IAdditiveIdentity<TVector, TVector>, IMultiplyOperators<TVector, TScalar, TVector>
        {
            var result = TVector.AdditiveIdentity;
            foreach (var (item, scalar) in source.Zip(scalars))
                result += vectorSelector(item) * scalar;
            return result;
        }

        public static TResult Sum<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : IAdditionOperators<TResult, TResult, TResult>, IAdditiveIdentity<TResult, TResult>
        {
            var result = TResult.AdditiveIdentity;
            foreach (var item in source)
                result += selector(item);
            return result;
        }

        public static T Sum<T>(this IEnumerable<T> source)
            where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
        {
            var result = T.AdditiveIdentity;
            foreach (var item in source)
                result += item;
            return result;
        }

        /// <summary>
        /// Throws exception if source is empty
        /// </summary>
        public static TResult MaxOrThrow<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : IComparisonOperators<TResult, TResult, bool>
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            TResult max = selector(enumerator.Current);
            while (enumerator.MoveNext())
                max = MyMathHelper.Max(max, selector(enumerator.Current));
            return max;
        }

        /// <summary>
        /// Uses min value if source is empty
        /// </summary>
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : IComparisonOperators<TResult, TResult, bool>, IMinMaxValue<TResult>
        {
            var result = TResult.MinValue;
            foreach (var item in source)
                result = MyMathHelper.Max(result, selector(item));
            return result;
        }

        /// <summary>
        /// Uses min value if source is empty
        /// </summary>
        public static TimeSpan MaxOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            var result = TimeSpan.MinValue;
            foreach (var item in source)
                result = MyMathHelper.Max(result, selector(item));
            return result;
        }

        public static TimeSpan Average<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            var sum = TimeSpan.Zero;
            uint count = 0;
            foreach (var item in source)
            {
                sum += selector(item);
                count++;
            }
            if (count is 0)
                return TimeSpan.Zero;
            else
                return sum / count;
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
            => source.ToArray();

        public static TSource? ArgMaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).LastOrDefault();

        public static TSource? ArgMinOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).FirstOrDefault();

        public static RealPeopleStats CombineRealPeopleStats<TSource>(this TSource source)
            where TSource : IEnumerable<IWithRealPeopleStats>
        {
            var result = RealPeopleStats.empty;
            foreach (var item in source)
                result = result.CombineWith(other: item.RealPeopleStats);
            return result;
        }

        public static Dictionary<TKey, double> ClampValues<TKey>(this IReadOnlyDictionary<TKey, double> dictionary, double min, double max)
            where TKey : notnull
            => dictionary.ToDictionary
            (
                keySelector: a => a.Key,
                elementSelector: a => MyMathHelper.Clamp(a.Value, min, max)
            );

        public static MySet<T> ToMyHashSet<T>(this IEnumerable<T> source)
        {
            MySet<T> result = new();
            foreach (var item in source)
                result.Add(item);
            return result;
        }

        public static bool Transparent(this Color color)
            => color.A is 0;

        public static string ToDebugString(this IEnumerable<object> source)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("{ ");
            foreach (var item in source)
            {
                stringBuilder.Append(item.ToString());
                stringBuilder.Append(", ");
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }
}
