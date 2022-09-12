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
                source.Count = source.Count.Subtract(count: count);
                Count = Count.Add(count: count);
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
            => new(massCounter: Counter<Mass>.CreateEmpty(), peopleCounter: Counter<NumPeople>.CreateEmpty());

        public static LocationCounters CreateOnePersonByMagic()
            => new
            (
                massCounter: Counter<Mass>.CreateEmpty(),
                peopleCounter: Counter<NumPeople>.CreateCounterByMagic(count: NumPeople.one)
            );

        public static LocationCounters CreateMassByMagic(Mass mass)
            => new
            (
                massCounter: Counter<Mass>.CreateCounterByMagic(count: mass),
                peopleCounter: Counter<NumPeople>.CreateEmpty()
            );

        public Mass Mass
            => massCounter.Count;
        public NumPeople NumPeople
            => peopleCounter.Count;

        private readonly Counter<Mass> massCounter;
        private readonly Counter<NumPeople> peopleCounter;

        private LocationCounters(Counter<Mass> massCounter, Counter<NumPeople> peopleCounter)
        {
            this.massCounter = massCounter;
            this.peopleCounter = peopleCounter;
        }

        public void TransferFrom(LocationCounters source, Mass mass, NumPeople numPeople)
        {
            massCounter.TransferFrom(source: source.massCounter, count: mass);
            peopleCounter.TransferFrom(source: source.peopleCounter, count: numPeople);
        }
    }
}
