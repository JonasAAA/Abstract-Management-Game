namespace Game1
{
    [Serializable]
    public sealed class SimpleHistoricProporSplitter<TKey>
        where TKey : notnull
    {
        private readonly HistoricProporSplitter<TKey> internalProporSplitter;

        public SimpleHistoricProporSplitter()
            => internalProporSplitter = new();

        public Dictionary<TKey, UInt96> Split(UInt96 amount, Dictionary<TKey, UDouble> importances)
        {
#warning Implement this without HistoricProporSplitter (there must be a much simpler way), and then delete HistoricProporSplitter
            foreach (var key in internalProporSplitter.Keys)
                if (!importances.ContainsKey(key))
                    internalProporSplitter.RemoveKey(key: key);
            foreach (var (key, importance) in importances)
                internalProporSplitter.SetImportance(key: key, importance: importance);

            var (splitAmounts, unsplitAmount) = internalProporSplitter.Split(amount: amount, maxAmountsFunc: key => UInt96.maxValue);
            Debug.Assert(unsplitAmount == 0);
            return splitAmounts;
        }
    }
}
