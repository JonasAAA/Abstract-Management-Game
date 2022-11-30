namespace Game1.Resources
{
    public interface IEnergySouce<T>
        where T : struct, IFormOfEnergy<T>
    {
        public T AvailableEnergy { get; }

        public void TransferEnergyTo(LocationCounters destin, T energy);
    }
}
