namespace Game1
{
    [Serializable]
    public readonly record struct LocationCounters
    {
        public static LocationCounters CreateEmpty()
            => new(counters: Counters.CreateEmpty());

        public static LocationCounters CreateCounterByMagic<TAmount>(TAmount amount)
            where TAmount : struct, ICountable<TAmount>
            => new(counters: Counters.CreateCounterByMagic(amount: amount));

        private readonly Counters counters;

        private LocationCounters(Counters counters)
            => this.counters = counters;

        public TAmount GetCount<TAmount>()
            where TAmount : struct, ICountable<TAmount>
            => counters.GetCount<TAmount>();

        public void TransferFrom<TAmount>(LocationCounters source, TAmount amount)
            where TAmount : struct, ICountable<TAmount>
            => counters.TransferFrom(source: source.counters, amount: amount);

        public void TransformFrom<TSourceAmount, TDestinAmount>(LocationCounters source, TSourceAmount sourceAmount)
            where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
            => counters.TransformFrom<TSourceAmount, TDestinAmount>(source: source.counters, sourceAmount: sourceAmount);

        public void TransformTo<TSourceAmount, TDestinAmount>(LocationCounters destin, TDestinAmount destinAmount)
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
            where TDestinAmount : struct, IFormOfEnergy<TDestinAmount>
            => counters.TransformTo<TSourceAmount, TDestinAmount>(destin: destin.counters, destinAmount: destinAmount);
    }
}
