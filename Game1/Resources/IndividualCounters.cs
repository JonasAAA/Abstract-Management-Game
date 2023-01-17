namespace Game1.Resources
{
    public readonly struct IndividualCounters : IPile<ElectricalEnergy>
    {
        public static IndividualCounters CreateEmpty(LocationCounters locationCounters)
            => new(counters: Counters.CreateEmpty(), locationCounters: locationCounters);

        public LocationCounters LocationCounters { get; }

        ElectricalEnergy ISourcePile<ElectricalEnergy>.Amount
            => throw new NotImplementedException();

        private readonly Counters counters;

        private IndividualCounters(Counters counters, LocationCounters locationCounters)
        {
            this.counters = counters;
            LocationCounters = locationCounters;
        }

        public void TransferFrom(IndividualCounters source, ElectricalEnergy amount)
            => TransferFrom<ElectricalEnergy>(source: source, amount: amount);

        public void TransferTo(IndividualCounters destin, ElectricalEnergy amount)
            => destin.TransferFrom(source: this, amount: amount);

        public void TransferFrom<T>(IndividualCounters source, T amount)
            where T : struct, ICountable<T>
        {
            counters.TransferFrom(source: source.counters, amount: amount);
            LocationCounters.TransferFrom(source: source.LocationCounters, amount: amount);
        }

        public void Transform<TFrom, TTo>(TFrom amount)
            where TFrom : struct, IFormOfEnergy<TFrom>
            where TTo : struct, IUnconstrainedEnergy<TTo>
        {
            counters.Transform<TFrom, TTo>(amount: amount);
            LocationCounters.Transform<TFrom, TTo>(amount: amount);
        }
    }
}
