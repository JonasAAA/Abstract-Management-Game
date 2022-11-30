namespace Game1.Resources
{
    public interface IEnergyDestin<T>
        where T : struct, IFormOfEnergy<T>
    {
        public void TransferEnergyFrom(LocationCounters source, T energy);
    }
}
