using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class ProporSplitter
    {
        public int Count
            => proportions.Count;

        public ReadOnlyCollection<double> Proportions
        {
            get => proportions;
            set
            {
                if (value.Any(a => a < 0))
                    throw new ArgumentException();
                double propSum = value.Sum();
                if (propSum is 0)
                    proportions = value;
                else
                    proportions = new((from a in value select a / propSum).ToArray());
                necAdds = new(collection: new double[Count]);
            }
        }

        private ReadOnlyCollection<double> proportions;
        private List<double> necAdds;

        public ProporSplitter()
        {
            Proportions = new(list: Array.Empty<double>());
            necAdds = new();
        }

        public void InsertVar(int index)
        {
            var tempProportions = proportions.ToList();
            tempProportions.Insert(index: index, item: 0);
            proportions = tempProportions.AsReadOnly();
            necAdds.Insert(index: index, item: 0);
        }

        public bool CanSplit(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException();

            return amount is 0 || proportions.Sum() is not 0;
        }

        public int[] Split(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException();

            if (!CanSplit(amount: amount))
                throw new Exception();

            if (amount is 0)
                return new int[Count];

            Debug.Assert(C.IsTiny(value: Proportions.Sum() - 1));

            var answer = new int[Count];
            var perfect = new double[Count];
            int unusedAmount = amount;
            for (int i = 0; i < Count; i++)
            {
                perfect[i] = amount * proportions[i] + necAdds[i];
                answer[i] = (int)perfect[i];
                necAdds[i] = perfect[i] - answer[i];
                unusedAmount -= answer[i];
            }

            var priorityInds = Enumerable.Range(start: 0, count: Count).OrderByDescending(a => necAdds[a]);
            foreach (int ind in priorityInds)
            {
                if (unusedAmount is 0)
                    break;
                answer[ind]++;
                necAdds[ind]--;
                unusedAmount--;
            }

            double necAddsSum = necAdds.Sum();
            necAdds = necAdds.Select(a => a - necAddsSum / Count).ToList();

            Debug.Assert(unusedAmount is 0);
            Debug.Assert(C.IsTiny(value: necAdds.Sum()));

            return answer;
        }
    }
}
