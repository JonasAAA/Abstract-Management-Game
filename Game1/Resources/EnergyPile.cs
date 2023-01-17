namespace Game1.Resources
{
    public class EnergyPile<TAmount> : IPile<TAmount>
        where TAmount : struct, IFormOfEnergy<TAmount>
    {
        public LocationCounters LocationCounters { get; }

        private readonly EnergyCounter<TAmount> counter;
    }
}
