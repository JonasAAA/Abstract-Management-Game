namespace Game1.Resources
{
    [Serializable]
    public readonly struct ThermalBody
    {
        public static ThermalBody CreateEmpty(LocationCounters locationCounters)
            => new
            (
                heatEnergyPile: EnergyPile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                resCounter: ResCounter.CreateEmpty()
            );

        public static ThermalBody CreateByMagic(LocationCounters locationCounters, ResAmounts amount)
            => new
            (
                heatEnergyPile: EnergyPile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                resCounter: ResCounter.CreateByMagic(count: amount)
            );

        public readonly LocationCounters locationCounters;

        private readonly EnergyPile<HeatEnergy> heatEnergyPile;
        // This may need to not be any counter, as when resources are transformed into radiant energy,
        // there is no counter to transfer that stuff to. After all, this counter is just to keep track
        // of resources in this thermal body, not to enforce conservation of energy
        private readonly ResCounter resCounter;

        private ThermalBody(EnergyPile<HeatEnergy> heatEnergyPile, ResCounter resCounter)
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

        public void TransformResFrom(ThermalBody source, ResRecipe recipe)
            => resCounter.TransformFrom(source: source.resCounter, recipe: recipe);

        public void TransformResTo(ThermalBody destin, ResRecipe recipe)
            => resCounter.TransformTo(destin: destin.resCounter, recipe: recipe);

        public void TransformAllElectricityToHeatAndTransferFrom(EnergyPile<ElectricalEnergy> source)
            => source.TransformAllTo(destin: heatEnergyPile);
    }
}
