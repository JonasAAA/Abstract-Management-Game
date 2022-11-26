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
                Count = default;
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
                massCounter: EnergyCounter<Mass>.CreateEmpty(),
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                heatCapacityCounter: Counter<HeatCapacity>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty()
            );

        public static LocationCounters CreatePersonCounterByMagic(NumPeople numPeople)
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateCounterByMagic(count: numPeople),
                massCounter: EnergyCounter<Mass>.CreateEmpty(),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateEmpty(),
                heatCapacityCounter: Counter<HeatCapacity>.CreateEmpty(),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty()
            );

        public static LocationCounters CreateResAmountsCountersByMagic(ResAmounts resAmounts, ulong temperatureInK)
        {
            var resHeatCapacity = resAmounts.HeatCapacity();
            // TODO: Look at this, want to insure that the (total amount of energy) * (max heat capacity) fit comfortably into ulong
            // If run into problems with overflow, could use int128 or uint128 instead of ulong from
            // https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0 https://learn.microsoft.com/en-us/dotnet/api/system.uint128?view=net-7.0
            return new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                massCounter: EnergyCounter<Mass>.CreateCounterByMagic(count: resAmounts.Mass()),
                heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateCounterByMagic(count: HeatEnergy.CreateFromJoules(valueInJ: temperatureInK * resHeatCapacity.valueInJPerK)),
                heatCapacityCounter: Counter<HeatCapacity>.CreateCounterByMagic(count: resHeatCapacity),
                radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty() 
            );
        }

        public NumPeople NumPeople
            => peopleCounter.Count;
        public HeatCapacity HeatCapacity
            => heatCapacityCounter.Count;
        public HeatEnergy HeatEnergy
            => heatEnergyCounter.Count;
        public Mass Mass
            => massCounter.Count;
        public RadiantEnergy RadiantEnergy
            => radiantEnergyCounter.Count;

        private readonly Counter<NumPeople> peopleCounter;
        private readonly Counter<HeatCapacity> heatCapacityCounter;
        private readonly EnergyCounter<HeatEnergy> heatEnergyCounter;
        private readonly EnergyCounter<Mass> massCounter;
        private readonly EnergyCounter<RadiantEnergy> radiantEnergyCounter;

        private LocationCounters(Counter<NumPeople> peopleCounter, Counter<HeatCapacity> heatCapacityCounter, EnergyCounter<HeatEnergy> heatEnergyCounter, EnergyCounter<Mass> massCounter, EnergyCounter<RadiantEnergy> radiantEnergyCounter)
        {
            this.peopleCounter = peopleCounter;
            this.heatCapacityCounter = heatCapacityCounter;
            this.heatEnergyCounter = heatEnergyCounter;
            this.massCounter = massCounter;
            this.radiantEnergyCounter = radiantEnergyCounter;
        }

        public void TransferPeopleFrom(LocationCounters source, NumPeople numPeople)
            => peopleCounter.TransferFrom(source: source.peopleCounter, count: numPeople);

        public void TransferResFrom(LocationCounters source, ResAmounts resAmounts)
        {
            massCounter.TransferFrom(source: source.massCounter, count: resAmounts.Mass());
            var resHeatCapacity = resAmounts.HeatCapacity();
            heatEnergyCounter.TransferFrom
            (
                source: source.heatEnergyCounter,
                count: HeatEnergy.CreateFromJoules
                (
                    valueInJ: MyMathHelper.RoundedDivision
                    (
                        dividend: source.HeatEnergy.ValueInJ * resHeatCapacity.valueInJPerK,
                        divisor: source.HeatCapacity.valueInJPerK
                    )
                )
            );
            heatCapacityCounter.TransferFrom(source: source.heatCapacityCounter, count: resHeatCapacity);
        }

        public void TransformMassToRadiantEnergy(LocationCounters source, Mass mass)
            => source.massCounter.TransformTo(destin: radiantEnergyCounter, count: mass);

        public void TransformRadiantEnergyToHeat(LocationCounters source, RadiantEnergy radiantEnergy)
            => source.radiantEnergyCounter.TransformTo(destin: heatEnergyCounter, count: radiantEnergy);
    }
}
