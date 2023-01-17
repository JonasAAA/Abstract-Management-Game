namespace Game1.Resources
{
    [Serializable]
    public class EnergyPile<T> : IEnergyDestin<T>, IEnergySouce<T>
        where T : struct, IFormOfEnergy<T>
    {
        public static EnergyPile<T> CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters);

        public T Energy { get; private set; }

        public readonly LocationCounters locationCounters;

        private EnergyPile(LocationCounters locationCounters)
        {
            this.locationCounters = locationCounters;
            Energy = T.AdditiveIdentity;
        }

        public void TransferEnergyFrom(EnergyPile<T> source, T energy)
        {
            throw new NotImplementedException();
        }

        public void TransferEnergyTo(EnergyPile<T> destin, T energy)
        {
            throw new NotImplementedException();
        }
    }
}
