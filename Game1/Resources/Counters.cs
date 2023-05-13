namespace Game1.Resources
{
    [Serializable]
    public class Counters
    {
        public static Counters CreateEmpty()
            => new();

        public static Counters CreateCounterByMagic<TAmount>(TAmount amount)
            where TAmount : struct, ICountable<TAmount>
            => amount switch
            {
                NumPeople numPeople => new()
                {
                    PeopleCounter = Counter<NumPeople>.CreateByMagic(count: numPeople)
                },
                AllResAmounts resAmounts => new()
                {
                    ResCounter = ResCounter.CreateByMagic(count: resAmounts)
                },
                HeatEnergy heatEnergy => new()
                {
                    HeatEnergyCounter = EnergyCounter<HeatEnergy>.CreateByMagic(count: heatEnergy)
                },
                RadiantEnergy radiantEnergy => new()
                {
                    RadiantEnergyCounter = EnergyCounter<RadiantEnergy>.CreateByMagic(count: radiantEnergy)
                },
                ElectricalEnergy electricalEnergy => new()
                {
                    ElectricalEnergyCounter = EnergyCounter<ElectricalEnergy>.CreateByMagic(count: electricalEnergy)
                },
                _ => throw new ArgumentException()
            };

        private Counter<NumPeople> PeopleCounter { get; init; }
        private EnergyCounter<HeatEnergy> HeatEnergyCounter { get; init; }
        private EnergyCounter<RadiantEnergy> RadiantEnergyCounter { get; init; }
        private EnergyCounter<ElectricalEnergy> ElectricalEnergyCounter { get; init; }
        private ResCounter ResCounter { get; init; }

        public Counters()
        {
            PeopleCounter = Counter<NumPeople>.CreateEmpty();
            HeatEnergyCounter = EnergyCounter<HeatEnergy>.CreateEmpty();
            RadiantEnergyCounter = EnergyCounter<RadiantEnergy>.CreateEmpty();
            ElectricalEnergyCounter = EnergyCounter<ElectricalEnergy>.CreateEmpty();
            ResCounter = ResCounter.CreateEmpty();
        }

        public void TransferFrom<TAmount>(Counters source, TAmount amount)
            where TAmount : struct, ICountable<TAmount>
        {
            if (this == source)
                return;
            GetCounter<TAmount>().TransferFrom
            (
                source: source.GetCounter<TAmount>(),
                count: amount
            );
        }

        public void TransferTo<TAmount>(Counters destin, TAmount amount)
            where TAmount : struct, ICountable<TAmount>
            => destin.TransferFrom(source: this, amount: amount);

        public void TransformFrom<TAmount, TSourceAmount>(Counters source, TAmount amount)
            where TAmount : struct, IFormOfEnergy<TAmount>
            where TSourceAmount : struct, IUnconstrainedEnergy<TSourceAmount>
            => GetEnergyCounter<TAmount>().TransformFrom
            (
                source: source.GetEnergyCounter<TSourceAmount>(),
                destinCount: amount
            );

        public void TransformTo<TAmount, TDestinAmount>(Counters destin, TAmount amount)
            where TAmount : struct, IFormOfEnergy<TAmount>
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
            => GetEnergyCounter<TAmount>().TransformTo
            (
                destin: destin.GetEnergyCounter<TDestinAmount>(),
                sourceCount: amount
            );

        public void TransformFrom(Counters source, ResRecipe recipe)
            => ResCounter.TransformFrom(source: source.ResCounter, recipe: recipe);

        public void TransformTo(Counters destin, ResRecipe recipe)
            => ResCounter.TransformTo(destin: destin.ResCounter, recipe: recipe);

        public TAmount GetCount<TAmount>()
            where TAmount : struct, ICountable<TAmount>
            => GetCounter<TAmount>().Count;

        private Counter<TAmount> GetCounter<TAmount>()
            where TAmount : struct, ICountable<TAmount>
        {
            // This default(TAmount) switch statement only works since TAmount is a struct.
            // If it were a class, then default would be null always, thus not holding any type information.
            object counter = default(TAmount) switch
            {
                NumPeople => PeopleCounter,
                AllResAmounts => ResCounter,
                HeatEnergy => HeatEnergyCounter,
                RadiantEnergy => RadiantEnergyCounter,
                ElectricalEnergy => ElectricalEnergyCounter,
                _ => throw new ArgumentException()
            };
            return (Counter<TAmount>)counter;
        }

        private EnergyCounter<TAmount> GetEnergyCounter<TAmount>()
            where TAmount : struct, IFormOfEnergy<TAmount>
            => (EnergyCounter<TAmount>)GetCounter<TAmount>();
    }
}
