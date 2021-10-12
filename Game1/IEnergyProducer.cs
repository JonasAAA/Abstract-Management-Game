using System;

namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyProducer() for each instance
    /// </summary>
    public interface IEnergyProducer : IDeletable
    {
        public double ProdWatts();
    }
}
