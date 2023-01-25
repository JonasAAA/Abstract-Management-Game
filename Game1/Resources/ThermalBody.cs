namespace Game1.Resources
{
    public readonly struct ThermalBody
    {
        private readonly Pile<HeatEnergy> heatEnergyPile;
        private readonly Pile<HeatCapacity> heatCapacityPile;

        public ThermalBody(Pile<HeatEnergy> heatEnergyPile, Pile<HeatCapacity> heatCapacityPile)
        {
            this.heatEnergyPile = heatEnergyPile;
            this.heatCapacityPile = heatCapacityPile;
        }
    }
}
