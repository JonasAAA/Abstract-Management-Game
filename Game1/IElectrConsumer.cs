using System;

namespace Game1
{
    /// <summary>
    /// MUST call ElectricityDistributor.AddElectrConsumer() for each instance
    /// </summary>
    public interface IElectrConsumer : IDeletable
    {
        /// <summary>
        /// the lower, the more important
        /// </summary>
        public ulong ElectrPriority { get; }

        public double ReqWattsPerSec();

        public void ConsumeElectr(double electrPropor);
    }
}
