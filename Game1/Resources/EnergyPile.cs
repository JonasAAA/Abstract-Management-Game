namespace Game1.Resources
{
    [Serializable]
    public class EnergyPile<T>
        where T : struct, IFormOfEnergy<T>
    {
        public static EnergyPile<T> CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters);

        public T Energy { get; private set; }

        private readonly LocationCounters locationCounters;

        private EnergyPile(LocationCounters locationCounters)
        {
            this.locationCounters = locationCounters;
            Energy = T.AdditiveIdentity;
        }
    }
}
