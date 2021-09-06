using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class ProporSplitter<TKey>
    {
        public bool Empty
            => Count is 0;
        public IEnumerable<TKey> Keys
            => proportions.Keys;
        public IReadOnlyDictionary<TKey, double> Proportions
        {
            get => proportions;
            //set
            //{
            //    if (value.Count != Count || value.Values.Any(a => a < 0))
            //        throw new ArgumentException();
            //    proportions = new(value);
            //    necAdds = Keys.ToDictionary
            //    (
            //        keySelector: key => key,
            //        elementSelector: key => .0
            //    );
            //    //double propSum = value.Values.Sum();
            //    //if (propSum is 0)
            //    //    proportions = new(value);
            //    //else
            //    //    proportions = value.ToDictionary
            //    //    (
            //    //        keySelector: a => a.Key,
            //    //        elementSelector: a => a.Value / propSum
            //    //    );

            //    //necAdds = value.Keys.ToDictionary
            //    //(
            //    //    keySelector: key => key,
            //    //    elementSelector: key => .0
            //    //);
            //}
        }

        private Dictionary<TKey, double> proportions;
        private Dictionary<TKey, double> necAdds;
        
        private int Count
            => proportions.Count;

        public ProporSplitter()
        {
            proportions = new();
            necAdds = new();
        }

        private bool ContainsKey(TKey key)
            => proportions.ContainsKey(key);

        private void AddKey(TKey key, double prop)
        {
            proportions.Add(key, prop);
            necAdds.Add(key, 0);
        }

        private void Remove(TKey key)
        {
            proportions.Remove(key);
            necAdds.Remove(key);

            double necAddsSum = necAdds.Values.Sum();
            necAdds = necAdds.ToDictionary
            (
                keySelector: a => a.Key,
                elementSelector: a => a.Value - necAddsSum / Count
            );
        }

        /// <returns> true if successful </returns>
        public bool AddToProp(TKey key, double add)
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

        public Dictionary<TKey, ulong> Split(ulong amount)
        {
            if (Empty)
                throw new InvalidOperationException();
            //if (!CanSplit(amount: amount))
            //    throw new ArgumentException();

            if (amount is 0)
                return Keys.ToDictionary
                (
                    keySelector: key => key,
                    elementSelector: key => (ulong)0
                );

            double propSum = proportions.Values.Sum();
            Dictionary<TKey, double> propsWithUnitSum = proportions.ToDictionary
            (
                keySelector: a => a.Key,
                elementSelector: a => a.Value / propSum
            );

            Debug.Assert(C.IsTiny(value: propsWithUnitSum.Values.Sum() - 1));

            Dictionary<TKey, ulong> answer = new();
            Dictionary<TKey, double> perfect = new();
            ulong unusedAmount = amount;
            foreach (var key in Keys)
            {
                perfect.Add(key, amount * propsWithUnitSum[key] + necAdds[key]);
                answer.Add(key, (ulong)perfect[key]);
                necAdds[key] = perfect[key] - answer[key];
                unusedAmount -= answer[key];
            }

            var priorityKeys = Keys.OrderByDescending(key => necAdds[key]);
            foreach (var key in priorityKeys)
            {
                if (unusedAmount is 0)
                    break;
                answer[key]++;
                necAdds[key]--;
                unusedAmount--;
            }

            double necAddsSum = necAdds.Values.Sum();
            Debug.Assert(C.IsTiny(necAddsSum));
            necAdds = necAdds.ToDictionary
            (
                keySelector: a => a.Key,
                elementSelector: a => a.Value - necAddsSum / Count
            );

            Debug.Assert(unusedAmount is 0);
            Debug.Assert(answer.Values.Sum() == amount);
            Debug.Assert(C.IsTiny(value: necAdds.Values.Sum()));

            return answer;
        }
    }
}


//namespace Game1
//{
//    public class ProporSplitter
//    {
//        public int Count
//            => proportions.Count;

//        public ReadOnlyCollection<double> Proportions
//        {
//            get => proportions;
//            set
//            {
//                if (value.Count != Count || value.Any(a => a < 0))
//                    throw new ArgumentException();
//                double propSum = value.Sum();
//                if (propSum is 0)
//                    proportions = value;
//                else
//                    proportions = new((from a in value select a / propSum).ToArray());
//                necAdds = new(collection: new double[Count]);
//            }
//        }

//        private ReadOnlyCollection<double> proportions;
//        private List<double> necAdds;

//        public ProporSplitter()
//        {
//            proportions = new(list: Array.Empty<double>());
//            necAdds = new();
//        }

//        public void InsertVar(int index)
//        {
//            var tempProportions = proportions.ToList();
//            tempProportions.Insert(index: index, item: 0);
//            proportions = tempProportions.AsReadOnly();
//            necAdds.Insert(index: index, item: 0);
//        }

//        public bool CanSplit(ulong amount)
//            => amount is 0 || proportions.Sum() is not 0;

//        public ulong[] Split(ulong amount)
//        {
//            if (!CanSplit(amount: amount))
//                throw new Exception();

//            if (amount is 0)
//                return new ulong[Count];

//            Debug.Assert(C.IsTiny(value: Proportions.Sum() - 1));

//            var answer = new ulong[Count];
//            var perfect = new double[Count];
//            ulong unusedAmount = amount;
//            for (int i = 0; i < Count; i++)
//            {
//                perfect[i] = amount * proportions[i] + necAdds[i];
//                answer[i] = (ulong)perfect[i];
//                necAdds[i] = perfect[i] - answer[i];
//                unusedAmount -= answer[i];
//            }

//            var priorityInds = Enumerable.Range(start: 0, count: Count).OrderByDescending(a => necAdds[a]);
//            foreach (int ind in priorityInds)
//            {
//                if (unusedAmount is 0)
//                    break;
//                answer[ind]++;
//                necAdds[ind]--;
//                unusedAmount--;
//            }

//            double necAddsSum = necAdds.Sum();
//            Debug.Assert(C.IsTiny(necAddsSum));
//            necAdds = necAdds.Select(a => a - necAddsSum / Count).ToList();

//            Debug.Assert(unusedAmount is 0);
//            Debug.Assert(answer.Sum() == amount);
//            Debug.Assert(C.IsTiny(value: necAdds.Sum()));

//            return answer;
//        }
//    }
//}
