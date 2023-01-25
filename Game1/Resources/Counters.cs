using System.Diagnostics.Metrics;

namespace Game1.Resources
{
    public readonly struct Counters
    {
        public static Counters CreateEmpty()
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                resCounter: EnergyCounter<ResAmounts>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty(),
                electricalEnergyCounter: EnergyCounter<ElectricalEnergy>.CreateEmpty()
            );

        public static Counters CreatePersonCounterByMagic(NumPeople numPeople)
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateCounterByMagic(count: numPeople),
                resCounter: EnergyCounter<ResAmounts>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty(),
                electricalEnergyCounter: EnergyCounter<ElectricalEnergy>.CreateEmpty()
            );

        public static Counters CreateResAmountsCountersByMagic(ResAmounts resAmounts, ulong temperatureInK)
            // TODO: Look at this, want to insure that the (total sourceAmount of energy) * (max heat capacity) fit comfortably into ulong
            // If run into problems with overflow, could use int128 or uint128 instead of ulong from
            // https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0 https://learn.microsoft.com/en-us/dotnet/api/system.uint128?view=net-7.0
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                resCounter: EnergyCounter<ResAmounts>.CreateCounterByMagic(count: resAmounts),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateCounterByMagic(count: HeatEnergy.CreateFromJoules(valueInJ: temperatureInK * resAmounts.HeatCapacity().valueInJPerK)),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty(),
                electricalEnergyCounter: EnergyCounter<ElectricalEnergy>.CreateEmpty()
            );

        //public NumPeople NumPeople
        //    => peopleCounter.Count;
        //public HeatCapacity HeatCapacity
        //    => resCounter.Count.HeatCapacity();
        //public HeatEnergy HeatEnergy
        //    => heatEnergyCounter.Count;
        //public Mass Mass
        //    => resCounter.Count.Mass();
        //public RadiantEnergy RadiantEnergy
        //    => radiantEnergyCounter.Count;
        //public ElectricalEnergy ElectricalEnergy
        //    => electricalEnergyCounter.Count;

        private readonly Counter<NumPeople> peopleCounter;
        private readonly EnergyCounter<ResAmounts> resCounter;
        private readonly EnergyCounter<HeatEnergy> heatEnergyCounter;
        private readonly EnergyCounter<RadiantEnergy> radiantEnergyCounter;
        private readonly EnergyCounter<ElectricalEnergy> electricalEnergyCounter;

        private Counters(Counter<NumPeople> peopleCounter, EnergyCounter<ResAmounts> resCounter, EnergyCounter<HeatEnergy> heatEnergyCounter,
            EnergyCounter<RadiantEnergy> radiantEnergyCounter, EnergyCounter<ElectricalEnergy> electricalEnergyCounter)
        {
            this.peopleCounter = peopleCounter;
            this.resCounter = resCounter;
            this.heatEnergyCounter = heatEnergyCounter;
            this.radiantEnergyCounter = radiantEnergyCounter;
            this.electricalEnergyCounter = electricalEnergyCounter;
        }

        public void TransferFrom<TAmount>(Counters source, TAmount amount)
            where T : struct, ICountable<T>
        {
            throw new NotImplementedException();
        }

        public void TransferTo<TAmount>(Counters destin, TAmount amount)
            => destin.TransferFrom(source: this, amount: amount);

        public void TransformFrom<TSourceAmount, TDestinAmount>(Counters source, TSourceAmount sourceAmount)
            where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            throw new NotImplementedException();
        }

        public void TransformTo<TSourceAmount, TDestinAmount>(Counters destin, TDestinAmount destinAmount)
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
            where TDestinAmount : struct, IFormOfEnergy<TDestinAmount>
        {
            throw new NotImplementedException();
        }

        public TAmount GetCount<TAmount>()
            where TAmount : struct, ICountable<TAmount>
            => GetCounter<TAmount>().Count;

        private Counter<TAmount> GetCounter<TAmount>()
            where TAmount : struct, ICountable<TAmount>
            => throw new NotImplementedException();

        //public void TransferPeopleFrom(Counters source, NumPeople numPeople)
        //    => peopleCounter.TransferFrom(source: source.peopleCounter, count: numPeople);

        //public void TransferResFrom(Counters source, ResAmounts resAmounts)
        //{
        //    // This must be done first to get accurate source heat capacity in the calculations
        //    heatEnergyCounter.TransferFrom
        //    (
        //        source: source.heatEnergyCounter,
        //        count: HeatEnergy.CreateFromJoules
        //        (
        //            valueInJ: MyMathHelper.MultThenDivideRound
        //            (
        //                factor1: source.HeatEnergy.ValueInJ,
        //                factor2: resAmounts.HeatCapacity().valueInJPerK,
        //                divisor: source.HeatCapacity.valueInJPerK
        //            )
        //        )
        //    );
        //    resCounter.TransferFrom(source: source.resCounter, count: resAmounts);
        //}

        //public void TransferRadiantEnergyFrom(Counters source, RadiantEnergy radiantEnergy)
        //    => radiantEnergyCounter.TransferFrom(source: source.radiantEnergyCounter, count: radiantEnergy);

        //public void TransformResToRadiantEnergy(ResAmounts resAmounts)
        //{
        //    // This should be called only from within EnergyPile
        //    throw new NotImplementedException();
        //    //  TODO: Maybe transfer appropriate sourceAmount of heat to radiant energy
        //    resCounter.TransformTo(destin: radiantEnergyCounter, count: resAmounts);
        //}

        ///// <returns>the sourceAmount of electrical energy transferred</returns>
        //public ElectricalEnergy TransformRadiantToElectricalEnergyAndTransfer<TAmount>(TAmount destin, Propor proporToTransform)
        //    where TAmount : IEnergyDestin<ElectricalEnergy>
        //{
        //    throw new NotImplementedException();
        //}

        ////public void TransformRadiantEnergyToHeat(RadiantEnergy radiantEnergy)
        ////    => radiantEnergyCounter.TransformTo(destin: heatEnergyCounter, count: radiantEnergy);
    }
}
