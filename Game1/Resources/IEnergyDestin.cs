namespace Game1.Resources
{
    public interface IEnergyDestin<T>
        where T : struct, IFormOfEnergy<T>
    {
        public void TransferEnergyFrom(EnergyPile<T> source, T energy);
    }
}
