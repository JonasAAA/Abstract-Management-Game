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

            public T Count { get; private set; }
#if DEBUG2
            private readonly bool createdByMagic;
#endif

            private Counter(bool createdByMagic)
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

        public static LocationCounters CreateEmpty()
            => new
            (
                massCounter: Counter<Mass>.CreateEmpty(),
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                heatEnergyCounter: Counter<HeatEnergy>.CreateEmpty(),
                heatCapacityCounter: Counter<HeatCapacity>.CreateEmpty()
            );

        public static LocationCounters CreatePersonCounterByMagic(NumPeople numPeople)
            => new
            (
                peopleCounter: Counter<NumPeople>.CreateCounterByMagic(count: numPeople),
                massCounter: Counter<Mass>.CreateEmpty(),
                heatEnergyCounter: Counter<HeatEnergy>.CreateEmpty(),
                heatCapacityCounter: Counter<HeatCapacity>.CreateEmpty()
            );

        public static LocationCounters CreateResAmountsCountersByMagic(ResAmounts resAmounts, UDouble temperatureInK)
        {
            var resHeatCapacity = resAmounts.HeatCapacity();
            return new
            (
                peopleCounter: Counter<NumPeople>.CreateEmpty(),
                massCounter: Counter<Mass>.CreateCounterByMagic(count: resAmounts.Mass()),
                heatEnergyCounter: Counter<HeatEnergy>.CreateCounterByMagic(count: HeatEnergy.CreateFromJoules(valueInJoules: temperatureInK * resHeatCapacity.valueInJPerK)),
                heatCapacityCounter: Counter<HeatCapacity>.CreateCounterByMagic(count: resHeatCapacity)
            );
        }

        public NumPeople NumPeople
            => peopleCounter.Count;
        public Mass Mass
            => massCounter.Count;
        public HeatEnergy HeatEnergy
            => heatEnergyCounter.Count;
        public HeatCapacity HeatCapacity
            => heatCapacityCounter.Count;

        private readonly Counter<NumPeople> peopleCounter;
        private readonly Counter<Mass> massCounter;
        private readonly Counter<HeatEnergy> heatEnergyCounter;
        private readonly Counter<HeatCapacity> heatCapacityCounter;

        private LocationCounters(Counter<NumPeople> peopleCounter, Counter<Mass> massCounter, Counter<HeatEnergy> heatEnergyCounter, Counter<HeatCapacity> heatCapacityCounter)
        {
            this.peopleCounter = peopleCounter;
            this.massCounter = massCounter;
            this.heatEnergyCounter = heatEnergyCounter;
            this.heatCapacityCounter = heatCapacityCounter;
        }

        public void TransferPeopleFrom(LocationCounters source, NumPeople numPeople)
            => peopleCounter.TransferFrom(source: source.peopleCounter, count: numPeople);

        public void TransferResFrom(LocationCounters source, ResAmounts resAmounts)
        {
            massCounter.TransferFrom(source: source.massCounter, count: resAmounts.Mass());
            var resHeatCapacity = resAmounts.HeatCapacity();
            heatEnergyCounter.TransferFrom(source: source.heatEnergyCounter, count: source.HeatEnergy * (resHeatCapacity / source.HeatCapacity));
            heatCapacityCounter.TransferFrom(source: source.heatCapacityCounter, count: resHeatCapacity);
        }
    }
}
