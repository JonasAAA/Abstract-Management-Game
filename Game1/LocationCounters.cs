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

        public void TransferTo<TAmount>(LocationCounters destin, TAmount amount)
            where TAmount : struct, ICountable<TAmount>
            => counters.TransferTo(destin: destin.counters, amount: amount);

        public void TransformFrom<TAmount, TSourceAmount>(LocationCounters source, TAmount amount)
            where TAmount : struct, IFormOfEnergy<TAmount>
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
            => counters.TransformFrom<TAmount, TSourceAmount>(source: source.counters, amount: amount);

        public void TransformTo<TAmount, TDestinAmount>(LocationCounters destin, TAmount amount)
            where TAmount : struct, IFormOfEnergy<TAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
            => counters.TransformTo<TAmount, TDestinAmount>(destin: destin.counters, amount: amount);

        public void TransformFrom(LocationCounters source, ResRecipe recipe)
            => counters.TransformFrom(source: source.counters, recipe: recipe);

        public void TransformTo(LocationCounters destin, ResRecipe recipe)
            => counters.TransformTo(destin: destin.counters, recipe: recipe);
    }
}
