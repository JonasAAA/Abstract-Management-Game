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
    }
}
