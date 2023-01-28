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

        private EnergyPile(LocationCounters locationCounters, EnergyCounter<TAmount> counter)
            : base(locationCounters: locationCounters, counter: counter)
        {
            Counter = counter;
        }

        public void TransformAllTo<TDestinAmount>(EnergyPile<TDestinAmount> destin)
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            TAmount amountToTransform = Amount;
            Counter.TransformTo(destin: destin.Counter, sourceCount: amountToTransform);
            destin.LocationCounters.TransformFrom<TAmount, TDestinAmount>(source: LocationCounters, sourceAmount: amountToTransform);
        }
    }

    //public static void TransformAllTo<TSourceAmount, TDestinAmount>(this ISourcePile<TSourceAmount> source, IDestinPile<TDestinAmount> destin)
    //    //where TSourcePile : ISourcePile<TSourceAmount>
    //    //where TDestinPile : IDestinPile<TDestinAmount>
    //    where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
    //    where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
    //{
    //    var energySource = EnergyPile<TSourceAmount>.CreateEmpty(LocationCounters: source.LocationCounters);
    //    energySource.TransferAllFrom(source: source);
    //    var energyDestin = EnergyPile<TDestinAmount>.CreateEmpty(LocationCounters: destin.LocationCounters);
    //    energySource.TransformAllTo(destin: energyDestin);
    //    destin.TransferAllFrom(source: energyDestin);
    //}
}
