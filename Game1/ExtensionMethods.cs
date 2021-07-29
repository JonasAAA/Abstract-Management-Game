using System.Collections.Generic;

namespace Game1
{
    public static class ExtensionMethods
    {
        public static uint Sum(this IEnumerable<uint> source)
        {
            uint sum = 0;
            foreach (var value in source)
                sum += value;
            return sum;
        }
    }
}
