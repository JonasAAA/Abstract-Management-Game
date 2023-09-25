using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public class ThermalBody
    {
        public static ThermalBody CreateEmpty(LocationCounters locationCounters)
            => new
            (
                heatEnergyPile: EnergyPile<HeatEnergy>.CreateEmpty(locationCounters: locationCounters),
                magicResAmounts: AllResAmounts.empty
            );

        public static ThermalBody CreateByMagic(LocationCounters locationCounters, AllResAmounts amount, Temperature temperature)
            => new
            (
                heatEnergyPile: EnergyPile<HeatEnergy>.CreateByMagic
                (
                    locationCounters: locationCounters,
                    amount: ResAndIndustryAlgos.HeatEnergyFromTemperature
                    (
                        temperature: temperature,
                        heatCapacity: amount.HeatCapacity()
                    )
                ),
                magicResAmounts: amount
            );

        public HeatEnergy HeatEnergy
            => heatEnergyPile.Amount;
        public HeatCapacity HeatCapacity
            => resAmounts.HeatCapacity();
        public readonly LocationCounters locationCounters;

        private readonly EnergyPile<HeatEnergy> heatEnergyPile;
        // This may need to not be any counter, as when resources are transformed into radiant energy,
        // there is no counter to transfer that stuff to. After all, this counter is just to keep track
        // of resources in this thermal body, not to enforce conservation of energy
        private AllResAmounts resAmounts;

        private ThermalBody(EnergyPile<HeatEnergy> heatEnergyPile, AllResAmounts magicResAmounts)
        {
            locationCounters = heatEnergyPile.LocationCounters;
            this.heatEnergyPile = heatEnergyPile;
            resAmounts = magicResAmounts;
        }

        public void TransferResFrom(ThermalBody source, AllResAmounts amount)
        {
            var sourceHeatCapacityInJPerK = source.resAmounts.HeatCapacity().valueInJPerK;
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

            source.resAmounts -= amount;
            resAmounts += amount;
        }

        public void TransformResFrom(ThermalBody source, ResRecipe recipe)
        {
            source.resAmounts -= recipe.ingredients;
            resAmounts += recipe.results;
        }

        public void TransformResTo(ThermalBody destin, ResRecipe recipe)
            => destin.TransformResFrom(source: this, recipe: recipe);

        public void TransformHeatEnergyTo<TDestinAmount>(EnergyPile<TDestinAmount> destin, TDestinAmount amount)
            where TDestinAmount : struct, IFormOfEnergy<TDestinAmount>
            => destin.TransformFrom(source: heatEnergyPile, amount: amount);

        public void TransformAllEnergyToHeatAndTransferFrom<TSourceAmount>(EnergyPile<TSourceAmount> source)
            where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
            => source.TransformAllTo(destin: heatEnergyPile);

        public void TransferHeatEnergyTo(EnergyPile<HeatEnergy> destin, HeatEnergy amount)
            => heatEnergyPile.TransferTo(destin: destin, amount: amount);

        /// <summary>
        /// Source must be from this thermal body
        /// If this reaction loses energy, it is assumed that thermal body contains enough heat to cover the difference
        /// </summary>
        public void PerformFusion(EnergyPile<AllResAmounts> source, RawMatAmounts results)
        {
            var initialResAmounts = source.Amount;
            source.TransformAllTo(destin: heatEnergyPile);
            resAmounts -= initialResAmounts;

            var finalResAmounts = results.ToAll();
            source.TransformFrom(source: heatEnergyPile, amount: finalResAmounts);
            resAmounts += finalResAmounts;
        }
    }
}
