﻿namespace Game1
{
    [Serializable]
    public sealed class HistoricProporSplitter<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// TODO:
        /// deal with potential numeric instability
        /// </summary>
        [Serializable]
        private sealed class NecAdds
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
                Debug.Assert(MyMathHelper.AreClose(sum, oldSum));

                necNotLockedAdds = necNotLockedAdds.Keys.ToDictionary
                (
                    elementSelector: key => this[key]
                );
                sum = necNotLockedAdds.Values.Sum();

                Debug.Assert(MyMathHelper.IsTiny(sum));
                // The max - min can be digger than 1 by quite a bit.
                // This can happen when some necAdd (from previous frame) is negative, and the importance is 0.
                //Debug.Assert(necNotLockedAdds.Count is 0 || necNotLockedAdds.Values.Max() - necNotLockedAdds.Values.Min() < 1 + MyMathHelper.minPosDecimal);
            }
        }

        public bool Empty
            => Count is 0;
        public IEnumerable<TKey> Keys
            => importances.Keys;
        public IReadOnlyDictionary<TKey, UDouble> Importances
            => importances;

        private readonly Dictionary<TKey, UDouble> importances;
        private readonly NecAdds necAdds;

        private int Count
            => importances.Count;

        public HistoricProporSplitter()
        {
            importances = new();
            necAdds = new();
        }

        public bool ContainsKey(TKey key)
            => importances.ContainsKey(key);

        private void AddKey(TKey key, UDouble importance)
        {
            importances.Add(key, importance);
            necAdds.Add(key: key);
        }

        public void RemoveKey(TKey key)
        {
            importances.Remove(key);
            necAdds.Remove(key: key);
        }

        public void SetImportance(TKey key, UDouble importance)
        {
            if (ContainsKey(key: key))
                importances[key] = importance;
            else
                AddKey(key: key, importance: importance);
        }

        public (Dictionary<TKey, UInt96> splitAmounts, UInt96 unsplitAmount) Split(UInt96 amount, Func<TKey, UInt96> maxAmountsFunc)
        {
            if (Empty)
                throw new InvalidOperationException();

            if (amount == 0)
                return
                (
                    splitAmounts: MakeDictionary(func: key => (UInt96)0),
                    unsplitAmount: 0
                );

            Dictionary<TKey, UInt96> maxAmounts = MakeDictionary(func: key => maxAmountsFunc(key)),
                splitAmounts = new();
            HashSet<TKey> unusedKeys = new(Keys);
            decimal unusedPropSum = (decimal)importances.Values.Sum();

            while (true)
            {
                bool didSomething = false;
                foreach (var key in unusedKeys.Clone())
                    if ((UInt96)(amount * (decimal)importances[key] / unusedPropSum + necAdds[key]) >= maxAmounts[key])
                    {
                        //could turn this into a function
                        necAdds.LockAtZero(key: key);
                        unusedKeys.Remove(key);
                        unusedPropSum -= (decimal)importances[key];
                        splitAmounts.Add(key, maxAmounts[key]);
                        amount -= maxAmounts[key];

                        didSomething = true;
                    }

                if (!didSomething)
                    break;
            }

            Dictionary<TKey, decimal> perfect = new();
            UInt96 splitAmount = amount;
            foreach (var key in unusedKeys)
            {
                perfect.Add(key, splitAmount * (decimal)importances[key] / unusedPropSum + necAdds[key]);
#warning Now that we know that (UInt96)perfect[key] doesn't always round down, need to ensure that will not give out more than have
                splitAmounts.Add(key, (UInt96)perfect[key]);
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
                if (amount == 0)
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

        private Dictionary<TKey, TValue> MakeDictionary<TValue>(Func<TKey, TValue> func, IEnumerable<TKey>? keys = null)
            => (keys ?? Keys).ToDictionary
            (
                elementSelector: func
            );
    }
}