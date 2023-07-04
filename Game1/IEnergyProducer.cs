namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyProducer() for each instance
    /// </summary>
    public interface IEnergyProducer
    {
        public void ProduceEnergy(EnergyPile<ElectricalEnergy> destin);
    }
}
