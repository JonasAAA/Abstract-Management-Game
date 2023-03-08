namespace Game1
{
    [Serializable]
    public sealed class SimpleHistoricProporSplitter<TKey>
        where TKey : notnull
    {
        private readonly HistoricProporSplitter<TKey> internalProporSplitter;

        public SimpleHistoricProporSplitter()
            => internalProporSplitter = new();

        public Dictionary<TKey, ulong> Split(ulong amount, Dictionary<TKey, UDouble> importances)
        {
            foreach (var key in internalProporSplitter.Keys)
                if (!importances.ContainsKey(key))
                    internalProporSplitter.RemoveKey(key: key);
            foreach (var (key, importance) in importances)
                internalProporSplitter.SetImportance(key: key, importance: importance);

            var (splitAmounts, unsplitAmount) = internalProporSplitter.Split(amount: amount, maxAmountsFunc: key => ulong.MaxValue);
            Debug.Assert(unsplitAmount is 0);
            return splitAmounts;
        }
    }
}
