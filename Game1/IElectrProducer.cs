namespace Game1
{
    /// <summary>
    /// MUST call ElectricityDistributor.AddElectrProducer() for each instance
    /// </summary>
    public interface IElectrProducer
    {
        public double ProdWattsPerSec();
    }
}
