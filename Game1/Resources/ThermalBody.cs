namespace Game1.Resources
{
    [Serializable]
    public readonly struct ThermalBody
    {
        public static ThermalBody CreateEmpty(LocationCounters locationCounters)
            => new
            (
                heatEnergyPile: Pile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                resCounter: Counter<ResAmounts>.CreateEmpty()
            );

        public static ThermalBody CreateByMagic(LocationCounters locationCounters, ResAmounts amount)
            => new
            (
                heatEnergyPile: Pile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                resCounter: Counter<ResAmounts>.CreateByMagic(count: amount)
            );

        public readonly LocationCounters locationCounters;

        private readonly Pile<HeatEnergy> heatEnergyPile;
        private readonly Counter<ResAmounts> resCounter;

        private ThermalBody(Pile<HeatEnergy> heatEnergyPile, Counter<ResAmounts> resCounter)
        {
            locationCounters = heatEnergyPile.LocationCounters;
            this.heatEnergyPile = heatEnergyPile;
            this.resCounter = resCounter;
        }

        public void TransferResFrom(ThermalBody source, ResAmounts amount)
        {
            var sourceHeatCapacityInJPerK = source.resCounter.Count.HeatCapacity().valueInJPerK;
            if (sourceHeatCapacityInJPerK > 0)
                //This must be done first to get accurate source heat capacity in the calculations
                heatEnergyPile.TransferFrom
                (
                    source: source.heatEnergyPile,
                    amount: HeatEnergy.CreateFromJoules
                    (
                        valueInJ: MyMathHelper.MultThenDivideRound
                        (
                            factor1: source.heatEnergyPile.Amount.ValueInJ,
                            factor2: amount.HeatCapacity().valueInJPerK,
                            divisor: sourceHeatCapacityInJPerK
                        )
                    )
                );
            resCounter.TransferFrom
            (
                source: source.resCounter,
                count: amount
            );
        }
    }
}
