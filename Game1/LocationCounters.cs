namespace Game1
{
    [Serializable]
    public readonly struct LocationCounters
    {
        [Serializable]
        private class Counter<T>
            where T : struct, ICountable<T>
        {
            public static Counter<T> CreateEmpty()
                => new(createdByMagic: false);

            public static Counter<T> CreateCounterByMagic(T count)
                => new(createdByMagic: true)
                {
                    Count = count
                };

            public T Count { get; protected set; }
#if DEBUG2
            private readonly bool createdByMagic;
#endif

            protected Counter(bool createdByMagic)
            {
                Count = T.AdditiveIdentity;
#if DEBUG2
                this.createdByMagic = createdByMagic;
#endif
            }

            public void TransferFrom(Counter<T> source, T count)
            {
                if (source == this)
                    return;
                source.Count -= count;
                Count += count;
            }

#if DEBUG2
            ~Counter()
            {
                if (!createdByMagic && !Count.IsZero)
                    throw new Exception();
            }
#endif
        }

        [Serializable]
        private class EnergyCounter<T> : Counter<T>
            where T : struct, IFormOfEnergy<T>
        {
            public new static EnergyCounter<T> CreateEmpty()
                => new(createdByMagic: false);

            public new static EnergyCounter<T> CreateCounterByMagic(T count)
                => new(createdByMagic: true)
                {
                    Count = count
                };

            private EnergyCounter(bool createdByMagic)
                : base(createdByMagic: createdByMagic)
            { }

            public void TransformFrom<U>(EnergyCounter<U> source, T count)
                where U : struct, IUnconstrainedFormOfEnergy<U>
            {
                source.Count -= U.CreateFromEnergy(energy: count.Energy);
                Count += count;
            }

            public void TransformTo<U>(EnergyCounter<U> destin, T count)
                where U : struct, IUnconstrainedFormOfEnergy<U>
            {
                destin.Count += U.CreateFromEnergy(energy: count.Energy);
                Count -= count;
            }
        }

        public static LocationCounters CreateEmpty()
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                resCounter: EnergyCounter<ResAmounts>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty()
            );

        public static LocationCounters CreatePersonCounterByMagic(NumPeople numPeople)
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateCounterByMagic(count: numPeople),
                resCounter: EnergyCounter<ResAmounts>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty()
            );

        public static LocationCounters CreateResAmountsCountersByMagic(ResAmounts resAmounts, ulong temperatureInK)
            // TODO: Look at this, want to insure that the (total amount of energy) * (max heat capacity) fit comfortably into ulong
            // If run into problems with overflow, could use int128 or uint128 instead of ulong from
            // https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0 https://learn.microsoft.com/en-us/dotnet/api/system.uint128?view=net-7.0
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                resCounter: EnergyCounter<ResAmounts>.CreateCounterByMagic(count: resAmounts),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateCounterByMagic(count: HeatEnergy.CreateFromJoules(valueInJ: temperatureInK * resAmounts.HeatCapacity().valueInJPerK)),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty() 
            );

        public NumPeople NumPeople
            => peopleCounter.Count;
        public HeatCapacity HeatCapacity
            => resCounter.Count.HeatCapacity();
        public HeatEnergy HeatEnergy
            => heatEnergyCounter.Count;
        public Mass Mass
            => resCounter.Count.Mass();
        public RadiantEnergy RadiantEnergy
            => radiantEnergyCounter.Count;

        private readonly Counter<NumPeople> peopleCounter;
        private readonly EnergyCounter<ResAmounts> resCounter;
        private readonly EnergyCounter<HeatEnergy> heatEnergyCounter;
        private readonly EnergyCounter<RadiantEnergy> radiantEnergyCounter;

        private LocationCounters(Counter<NumPeople> peopleCounter, EnergyCounter<ResAmounts> resCounter, EnergyCounter<HeatEnergy> heatEnergyCounter, EnergyCounter<RadiantEnergy> radiantEnergyCounter)
        {
            this.peopleCounter = peopleCounter;
            this.resCounter = resCounter;
            this.heatEnergyCounter = heatEnergyCounter;
            this.radiantEnergyCounter = radiantEnergyCounter;
        }

        public void TransferPeopleFrom(LocationCounters source, NumPeople numPeople)
            => peopleCounter.TransferFrom(source: source.peopleCounter, count: numPeople);

        public void TransferResFrom(LocationCounters source, ResAmounts resAmounts)
        {
            // This must be done first to get accurate source heat capacity in the calculations
            heatEnergyCounter.TransferFrom
            (
                source: source.heatEnergyCounter,
                count: HeatEnergy.CreateFromJoules
                (
                    valueInJ: MyMathHelper.MultThenDivideRoundDown
                    (
                        factor1: source.HeatEnergy.ValueInJ,
                        factor2: resAmounts.HeatCapacity().valueInJPerK,
                        divisor: source.HeatCapacity.valueInJPerK
                    )
                )
            );
            resCounter.TransferFrom(source: source.resCounter, count: resAmounts);
        }

        public void TransformResToRadiantEnergy(ResAmounts resAmounts)
            //  TODO: Maybe transfer appropriate amount of heat to radiant energy
            => resCounter.TransformTo(destin: radiantEnergyCounter, count: resAmounts);

        public void TransformRadiantEnergyToHeat(RadiantEnergy radiantEnergy)
            => radiantEnergyCounter.TransformTo(destin: heatEnergyCounter, count: radiantEnergy);
    }
}
