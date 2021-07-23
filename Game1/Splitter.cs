using System;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Splitter
    {
        private readonly int length;
        private readonly double[] proportions, necAdds;

        public Splitter(double[] proportions)
        {
            length = proportions.Length;
            double propSum = proportions.Sum();
            if (propSum is 0)
                throw new ArgumentException();

            this.proportions = proportions.Select(a => a / propSum).ToArray();
            necAdds = new double[length];
        }

        public int[] Split(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException();

            var answer = new int[length];
            var perfect = new double[length];
            int unusedAmount = amount;
            for (int i = 0; i < length; i++)
            {
                perfect[i] = amount * proportions[i] + necAdds[i];
                answer[i] = (int)perfect[i];
                necAdds[i] = perfect[i] - answer[i];
                unusedAmount -= answer[i];
            }

            var priorityInds = Enumerable.Range(start: 0, count: length).OrderByDescending(a => necAdds[a]);
            foreach (int ind in priorityInds)
            {
                if (unusedAmount is 0)
                    break;
                answer[ind]++;
                necAdds[ind]--;
                unusedAmount--;
            }

            Debug.Assert(unusedAmount is 0);
            Debug.Assert(C.IsTiny(value: necAdds.Sum()));

            return answer;
        }
    }
}
