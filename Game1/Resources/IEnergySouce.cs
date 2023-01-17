namespace Game1.Resources
{
    public interface IEnergySouce<T>
        where T : struct, IFormOfEnergy<T>
    {
        public T Energy { get; }

        public void TransferEnergyTo(EnergyPile<T> destin, T energy);
    }
}
