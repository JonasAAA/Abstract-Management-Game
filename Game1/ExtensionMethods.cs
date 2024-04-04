using Game1.Collections;
using Game1.Industries;
using Game1.Inhabitants;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace Game1
{
    public static class ExtensionMethods
    {
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
            where TKey : notnull
            where TValue : class, new()
            => dict.TryGetValue(key, out var value) switch
            {
                true => value,
                false => dict[key] = new()
            };

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
            where TResult : IMax<TResult>
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
            where TResult : IMax<TResult>, IMinMaxValue<TResult>
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

        public static UDouble WeightedAverage<TSource>(this IEnumerable<TSource> source, Func<TSource, (UDouble weight, UDouble value)> selector)
        {
            UDouble weightSum = 0, valueSum = 0;
            foreach (var item in source)
            {
                var (weight, value) = selector(item);
                weightSum += weight;
                valueSum += weight * value;
            }
            return valueSum / weightSum;
        }

        public static Propor WeightedAverage<TSource>(this IEnumerable<TSource> source, Func<TSource, (UDouble weight, Propor value)> selector)
        {
            UDouble weightSum = 0, valueSum = 0;
            foreach (var item in source)
            {
                var (weight, value) = selector(item);
                weightSum += weight;
                valueSum += weight * (UDouble)value;
            }
            return Propor.Create(part: valueSum, whole: weightSum)!.Value;
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
            => source.ToList();

        public static TValue? GetValueOrDefault<TDict, TKey, TValue>(this TDict dictionary, TKey key)
            where TDict : IReadOnlyDictionary<TKey, TValue>
            => dictionary.TryGetValue(key: key, out var value) ? value : default;

        public static TSource? ArgMaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).LastOrDefault();

        public static TSource? ArgMinOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).FirstOrDefault();

        public static RealPeopleStats CombineRealPeopleStats<TSource>(this TSource source)
            where TSource : IEnumerable<IWithRealPeopleStats>
        {
            var result = RealPeopleStats.empty;
            foreach (var item in source)
                result = result.CombineWith(other: item.Stats);
            return result;
        }

        public static EfficientReadOnlyCollection<TSource> ToEfficientReadOnlyCollection<TSource>(this IEnumerable<TSource> source)
            => new(list: source.ToList());

        public static EfficientReadOnlyCollection<TSource> ToEfficientReadOnlyCollection<TSource>(this List<TSource> source)
            => new(list: source);

        public static EfficientReadOnlyHashSet<TSource> ToEfficientReadOnlyHashSet<TSource>(this IEnumerable<TSource> source)
            => new(set: source.ToHashSet());

        public static EfficientReadOnlyDictionary<TKey, TValue> ToEfficientReadOnlyDict<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector)
            where TKey : notnull
            => new(source.ToDictionary(keySelector: keySelector, elementSelector: elementSelector));

        public static EfficientReadOnlyDictionary<TSource, TValue> ToEfficientReadOnlyDict<TSource, TValue>(this IEnumerable<TValue> elements, Func<TValue, TSource> keySelector)
            where TSource : notnull
            => new(elements.ToDictionary(keySelector: keySelector));

        public static EfficientReadOnlyDictionary<TSource, TValue> ToEfficientReadOnlyDict<TSource, TValue>(this IEnumerable<TSource> keys, Func<TSource, TValue> elementSelector)
            where TSource : notnull
            => new(keys.ToDictionary(elementSelector: elementSelector));

        public static Dictionary<TSource, TValue> ToDictionary<TSource, TValue>(this IEnumerable<TSource> keys, Func<TSource, TValue> elementSelector)
            where TSource : notnull
            => keys.ToDictionary(keySelector: key => key, elementSelector: elementSelector);

        public static ThrowingSet<T> ToThrowingSet<T>(this IEnumerable<T> source)
            => [.. source];

        public static void Replace<TItem>(this IList<TItem> list, TItem oldItem, TItem newItem)
            => list[list.IndexOf(oldItem)] = newItem;

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

        /// <returns>The amount of energy transfered to destin</returns>
        public static TAmount TransferProporTo<TAmount>(this EnergyPile<TAmount> source, EnergyPile<TAmount> destin, Propor propor)
            where TAmount : struct, IUnconstrainedEnergy<TAmount>
        {
            TAmount amountToTransfer = Algorithms.ScaleEnergy
            (
                amount: source.Amount,
                scale: (UDouble)propor
            );
            source.TransferTo(destin: destin, amount: amountToTransfer);
            return amountToTransfer;
        }

        /// <returns>The amount of energy transfered to destin</returns>
        public static TDestinAmount TransformProporTo<TSourceAmount, TDestinAmount>(this EnergyPile<TSourceAmount> source, EnergyPile<TDestinAmount> destin, Propor propor)
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            TSourceAmount amountToTransform = Algorithms.ScaleEnergy
            (
                amount: source.Amount,
                scale: (UDouble)propor
            );
            source.TransformTo(destin: destin, amount: amountToTransform);
            return TDestinAmount.CreateFromEnergy(energy: (Energy)amountToTransform);
        }

        public static NeighborDir Opposite(this NeighborDir neighborDir)
            => neighborDir switch
            {
                NeighborDir.In => NeighborDir.Out,
                NeighborDir.Out => NeighborDir.In
            };

        public static bool ContainsDuplicates<TItem>(this IEnumerable<TItem> items)
        {
            HashSet<TItem> itemSet = new();
            foreach (var item in items)
                if (!itemSet.Add(item))
                    return true;
            return false;
        }

        public static ImmutableHashSet<TItem> ToggleElement<TItem>(this ImmutableHashSet<TItem> set, TItem item)
#pragma warning disable CA1868 // Unnecessary call to 'Contains(item)'. This is the neatest way I can think of to express this idea
            => set.Contains(item) ? set.Remove(item) : set.Add(item);
#pragma warning restore CA1868 // Unnecessary call to 'Contains(item)'
    }
}
