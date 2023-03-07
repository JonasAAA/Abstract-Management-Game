﻿namespace Game1.Lighting
{
    public interface IRadiantEnergyConsumer
    {
        // May have Propor powerPropor parameter as well
        public void TakeRadiantEnergyFrom(EnergyPile<RadiantEnergy> source, RadiantEnergy amount);
    }
}