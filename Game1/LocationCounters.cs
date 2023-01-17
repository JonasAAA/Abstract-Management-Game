namespace Game1
{
    [Serializable]
    public readonly struct LocationCounters
    {
        public static LocationCounters CreateEmpty()
            => new(counters: Counters.CreateEmpty());

        private readonly Counters counters;

        private LocationCounters(Counters counters)
            => this.counters = counters;

        public T GetCount<T>()
            where T : struct, ICountable<T>
            => counters.GetCount<T>();

        public void TransferFrom<T>(LocationCounters source, T amount)
            where T : struct, ICountable<T>
            => counters.TransferFrom(source: source.counters, amount: amount);

        public void Transform<TFrom, TTo>(TFrom amount)
            where TFrom : struct, IFormOfEnergy<TFrom>
            where TTo : struct, IUnconstrainedEnergy<TTo>
            => counters.Transform<TFrom, TTo>(amount: amount);
    }
}
