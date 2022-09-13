using Game1.Inhabitants;
using System.Text;

namespace Game1
{
    public static class ExtensionMethods
    {
        public static ulong Sum(this IEnumerable<ulong> source)
        {
            ulong sum = 0;
            foreach (var value in source)
                sum += value;

            return sum;
        }

        public static UDouble Sum(this IEnumerable<UDouble> source)
        {
            UDouble sum = 0;
            foreach (var value in source)
                sum += value;
            return sum;
        }

        // could be optimized a la https://stackoverflow.com/questions/11030109/aggregate-vs-sum-performance-in-linq
        public static ulong Sum<T>(this IEnumerable<T> source, Func<T, ulong> selector)
            => source.Select(selector).Sum();

        public static UDouble Sum<T>(this IEnumerable<T> source, Func<T, UDouble> selector)
            => source.Select(selector).Sum();

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TResult : IMaxable<TResult>
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException();

            TResult max = selector(enumerator.Current);
            while (enumerator.MoveNext())
                max = MyMathHelper.Max(max, selector(enumerator.Current));
            return max;
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
