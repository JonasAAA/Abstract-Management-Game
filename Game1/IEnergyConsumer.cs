﻿using Game1.PrimitiveTypeWrappers;

namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyConsumer() for each instance
    /// </summary>
    public interface IEnergyConsumer : IDeletable
    {
        /// <summary>
        /// the lower, the more important
        /// </summary>
        public EnergyPriority EnergyPriority { get; }

        /// <summary>
        /// node position from which consume energy
        /// </summary>
        public Vector2 NodePos { get; }

        public double ReqWatts();

        public void ConsumeEnergy(double energyPropor);
    }
}
