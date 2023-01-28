namespace Game1.Resources
{
    [Serializable]
    public readonly struct ThermalBody
    {
        public static ThermalBody CreateEmpty(LocationCounters locationCounters)
            => new
            (
                heatEnergyPile: Pile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                heatCapacityPile: Pile<HeatCapacity>.CreateEmpty(locationCounters: locationCounters)
            );

        public readonly LocationCounters locationCounters;

        private readonly Pile<HeatEnergy> heatEnergyPile;
        private readonly Pile<HeatCapacity> heatCapacityPile;

        private ThermalBody(Pile<HeatEnergy> heatEnergyPile, Pile<HeatCapacity> heatCapacityPile)
        {
            Debug.Assert(heatEnergyPile.LocationCounters == heatCapacityPile.LocationCounters);
            locationCounters = heatEnergyPile.LocationCounters;
            this.heatEnergyPile = heatEnergyPile;
            this.heatCapacityPile = heatCapacityPile;
        }

        public void TransferResFrom(ThermalBody source, ResAmounts amount)
        {
            HeatCapacity heatCapacityInAmount = amount.HeatCapacity();

            //This must be done first to get accurate source heat capacity in the calculations
            heatEnergyPile.TransferFrom
            (
                source: source.heatEnergyPile,
                amount: HeatEnergy.CreateFromJoules
                (
                    valueInJ: MyMathHelper.MultThenDivideRound
                    (
                        factor1: source.heatEnergyPile.Amount.ValueInJ,
                        factor2: heatCapacityInAmount.valueInJPerK,
                        divisor: source.heatCapacityPile.Amount.valueInJPerK
                    )
                )
            );
            heatCapacityPile.TransferFrom
            (
                source: source.heatCapacityPile,
                amount: heatCapacityInAmount
            );
        }
    }
}
