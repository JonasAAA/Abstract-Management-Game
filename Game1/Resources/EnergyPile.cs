namespace Game1.Resources
{
    [Serializable]
    public class EnergyPile<TAmount> : Pile<TAmount>
        where TAmount : struct, IFormOfEnergy<TAmount>
    {
        public new static EnergyPile<TAmount> CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters, counter: EnergyCounter<TAmount>.CreateEmpty());

        public static EnergyPile<TAmount>? CreateIfHaveEnough(EnergyPile<TAmount> source, TAmount amount)
        {
            if (source.Amount >= amount)
            {
                EnergyPile<TAmount> newPile = new(locationCounters: source.LocationCounters, counter: EnergyCounter<TAmount>.CreateEmpty());
                newPile.TransferFrom(source: source, amount: amount);
                return newPile;
            }
            return null;
        }

        public static EnergyPile<TAmount> CreateByMagic(TAmount amount)
            => new
            (
                locationCounters: LocationCounters.CreateCounterByMagic(amount: amount),
                counter: EnergyCounter<TAmount>.CreateByMagic(count: amount)
            );

        protected override EnergyCounter<TAmount> Counter { get; }

        protected EnergyPile(LocationCounters locationCounters, EnergyCounter<TAmount> counter)
            : base(locationCounters: locationCounters, counter: counter)
        {
            Counter = counter;
        }

        public void TransformTo<TDestinAmount>(EnergyPile<TDestinAmount> destin, TAmount amount)
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            Counter.TransformTo(destin: destin.Counter, sourceCount: amount);
            LocationCounters.TransformTo<TAmount, TDestinAmount>(destin: destin.LocationCounters, amount: amount);
        }

        public void TransformFrom<TSourceAmount>(EnergyPile<TSourceAmount> source, TAmount amount)
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
        {
            Counter.TransformFrom(source: source.Counter, destinCount: amount);
            LocationCounters.TransformFrom<TAmount, TSourceAmount>(source: source.LocationCounters, amount: amount);
        }

        public void TransformAllTo<TDestinAmount>(EnergyPile<TDestinAmount> destin)
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
            => TransformTo(destin: destin, amount: Amount);
    }
}
