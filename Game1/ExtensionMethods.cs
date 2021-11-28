using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // could be optimized a la https://stackoverflow.com/questions/11030109/aggregate-vs-sum-performance-in-linq
        public static ulong Sum<T>(this IEnumerable<T> source, Func<T, ulong> selector)
            => source.Select(selector).Sum();

        public static ulong TotalWeight(this IEnumerable<Person> people)
            => people.Sum(person => person.weight);

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> source)
            => source.ToArray();

        public static TSource ArgMaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).LastOrDefault();

        public static TSource ArgMinOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.OrderBy(value => selector(value)).FirstOrDefault();

        public static Dictionary<TKey, double> ClampValues<TKey>(this IReadOnlyDictionary<TKey, double> dictionary, double min, double max)
            => dictionary.ToDictionary
            (
                keySelector: a => a.Key,
                elementSelector: a => Math.Clamp(a.Value, min, max)
            );

        public static Dictionary<T> ToMyHashSet<T>(this IEnumerable<T> source)
        {
            Dictionary<T> result = new();
            foreach (var item in source)
                result.Add(item);
            return result;
        }

        public static bool Transparent(this Color color)
            => color.A is 0;
    }
}
