using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class ProporSplitter<TKey>
    {
        /// <summary>
        /// TODO:
        /// deal with potential numeric instability
        /// </summary>
        private class NecAdds
        {
            private readonly HashSet<TKey> keys;
            private Dictionary<TKey, decimal> necNotLockedAdds;
            private decimal sum;

            public NecAdds()
            {
                keys = new();
                necNotLockedAdds = new();
                sum = 0;
            }

            public void LockAtZero(TKey key)
            {
                if (!necNotLockedAdds.ContainsKey(key))
                    return;
                
                sum -= necNotLockedAdds[key];
                necNotLockedAdds.Remove(key);
            }

            public void UnlockAll()
            {
                necNotLockedAdds = keys.ToDictionary
                (
                    keySelector: key => key,
                    elementSelector: key => this[key]
                );
                sum = necNotLockedAdds.Values.Sum();
            }

            public void SetNotLocked(Dictionary<TKey, decimal> necAdds)
            {
                if (necAdds.Count != necNotLockedAdds.Count)
                    throw new ArgumentException();

                // this also checks if keys are the same
                foreach (var key in necAdds.Keys)
                    necNotLockedAdds[key] = necAdds[key];

                sum = necNotLockedAdds.Values.Sum();
                Recallibrate();
            }

            public decimal this[TKey key]
                => necNotLockedAdds.ContainsKey(key) switch
                {
                    true => necNotLockedAdds[key] - sum / necNotLockedAdds.Count,
                    false => 0
                };

            /// <summary>
            /// O(n)
            /// </summary>
            public void Add(TKey key)
            {
                Recallibrate();
                keys.Add(key);
                necNotLockedAdds.Add(key: key, value: 0);
            }

            public void Remove(TKey key)
            {
                keys.Remove(key);
                sum -= necNotLockedAdds[key];
                necNotLockedAdds.Remove(key);
            }

            private void Recallibrate()
            {
                decimal oldSum = sum;
                sum = necNotLockedAdds.Values.Sum();
                Debug.Assert(C.IsTiny(sum - oldSum));

                necNotLockedAdds = necNotLockedAdds.Keys.ToDictionary
                (
                    keySelector: key => key,
                    elementSelector: key => this[key]
                );
                sum = necNotLockedAdds.Values.Sum();

                Debug.Assert(C.IsTiny(sum));
                Debug.Assert(necNotLockedAdds.Count is 0 || necNotLockedAdds.Values.Max() - necNotLockedAdds.Values.Min() < 1);
            }
        }

        public bool Empty
            => Count is 0;
        public IEnumerable<TKey> Keys
            => proportions.Keys;
        public IReadOnlyDictionary<TKey, decimal> Proportions
            => proportions;

        private readonly Dictionary<TKey, decimal> proportions;
        private readonly NecAdds necAdds;

        private int Count
            => proportions.Count;

        public ProporSplitter()
        {
            proportions = new();
            necAdds = new();
        }

        private bool ContainsKey(TKey key)
            => proportions.ContainsKey(key);

        private void AddKey(TKey key, decimal prop)
        {
            proportions.Add(key, prop);
            necAdds.Add(key: key);
        }

        private void Remove(TKey key)
        {
            proportions.Remove(key);
            necAdds.Remove(key: key);
        }

        /// <returns> true if successful </returns>
        public bool AddToProp(TKey key, decimal add)
        {
            if (!ContainsKey(key: key))
                AddKey(key: key, prop: 0);
            proportions[key] += add;
            if (C.IsTiny(value: proportions[key]))
            {
                Remove(key: key);
                return true;
            }
            if (proportions[key] < 0)
            {
                Remove(key: key);
                return false;
            }
            return true;
        }

        public void SetProp(TKey key, decimal value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException();
            if (ContainsKey(key: key))
                proportions[key] = value;
            else
                AddKey(key: key, prop: value);
            if (C.IsTiny(value: proportions[key]))
                Remove(key: key);
        }

        public (Dictionary<TKey, ulong> splitAmounts, ulong unsplitAmount) Split(ulong amount, Func<TKey, ulong> maxAmountsFunc)
        {
            if (Empty)
                throw new InvalidOperationException();

            if (amount is 0)
                return
                (
                    splitAmounts: MakeDictionary(func: key => (ulong)0),
                    unsplitAmount: 0
                );

            Dictionary<TKey, ulong> maxAmounts = MakeDictionary(func: key => maxAmountsFunc(key)),
                splitAmounts = new();
            HashSet<TKey> unusedKeys = new(Keys);
            decimal unusedPropSum = proportions.Values.Sum();

            while (true)
            {
                bool didSomething = false;
                foreach (var key in unusedKeys.Clone())
                    if ((ulong)(amount * proportions[key] / unusedPropSum + necAdds[key]) >= maxAmounts[key])
                    {
                        //could turn this into a function
                        necAdds.LockAtZero(key: key);
                        unusedKeys.Remove(key);
                        unusedPropSum -= proportions[key];
                        splitAmounts.Add(key, maxAmounts[key]);
                        amount -= maxAmounts[key];

                        didSomething = true;
                    }

                if (!didSomething)
                    break;
            }

            Dictionary<TKey, decimal> perfect = new();
            ulong splitAmount = amount;
            foreach (var key in unusedKeys)
            {
                perfect.Add(key, splitAmount * proportions[key] / unusedPropSum + necAdds[key]);
                splitAmounts.Add(key, (ulong)perfect[key]);
                amount -= splitAmounts[key];
            }

            Dictionary<TKey, decimal> tempNecAdds = MakeDictionary
            (
                keys: unusedKeys,
                func: key => perfect[key] - splitAmounts[key]
            );

            var priorityKeys = unusedKeys.OrderByDescending(key => tempNecAdds[key]);
            foreach (var key in priorityKeys)
            {
                if (amount is 0)
                    break;
                splitAmounts[key]++;
                tempNecAdds[key]--;
                amount--;
            }

            necAdds.SetNotLocked(necAdds: tempNecAdds);
            necAdds.UnlockAll();

            foreach (var key in Keys)
                Debug.Assert(splitAmounts[key] <= maxAmounts[key]);

            return
            (
                splitAmounts: splitAmounts,
                unsplitAmount: amount
            );
        }

        private Dictionary<TKey, TValue> MakeDictionary<TValue>(Func<TKey, TValue> func, IEnumerable<TKey> keys = null)
            => (keys ?? Keys).ToDictionary
            (
                keySelector: key => key,
                elementSelector: key => func(key)
            );
    }
}