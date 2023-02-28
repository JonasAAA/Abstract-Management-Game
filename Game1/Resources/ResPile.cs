namespace Game1.Resources
{
    [Serializable]
    public class ResPile
    {
        [Serializable]
        private sealed class ResPileInternal : EnergyPile<ResAmounts>
        {
            public new static ResPileInternal CreateEmpty(LocationCounters locationCounters)
                => new(locationCounters: locationCounters, counter: ResCounter.CreateEmpty());

            public static ResPileInternal? CreateIfHaveEnough(ResPileInternal source, ResAmounts amount)
            {
                if (source.Amount >= amount)
                {
                    ResPileInternal newPile = new(locationCounters: source.LocationCounters, counter: ResCounter.CreateEmpty());
                    newPile.TransferFrom(source: source, amount: amount);
                    return newPile;
                }
                return null;
            }

            public new static ResPileInternal CreateByMagic(ResAmounts amount)
                => new
                (
                    locationCounters: LocationCounters.CreateCounterByMagic(amount: amount),
                    counter: ResCounter.CreateByMagic(count: amount)
                );

            protected override ResCounter Counter { get; }

            private ResPileInternal(LocationCounters locationCounters, ResCounter counter)
                : base(locationCounters: locationCounters, counter: counter)
            {
                Counter = counter;
            }

            public void TransformFrom(ResPileInternal source, ResRecipe recipe)
                => Counter.TransformFrom(source: source.Counter, recipe: recipe);

            public void TransformTo(ResPileInternal destin, ResRecipe recipe)
                => Counter.TransformTo(destin: destin.Counter, recipe: recipe);
        }

        public static ResPile CreateEmpty(ThermalBody thermalBody)
            => new
            (
                resPileInternal: ResPileInternal.CreateEmpty(locationCounters: thermalBody.locationCounters),
                thermalBody: thermalBody
            );

        public static ResPile? CreateIfHaveEnough(ResPile source, ResAmounts amount)
        {
            var newPile = ResPileInternal.CreateIfHaveEnough(source: source.resPileInternal, amount: amount);
            if (newPile is null)
                return null;
            return new(resPileInternal: newPile, thermalBody: source.thermalBody);
        }

        public static ResPile CreateByMagic(ResAmounts amount)
        {
            // TODO: Look at this, want to insure that the (total amount of energy) * (max heat capacity) fit comfortably into ulong
            // If run into problems with overflow, could use int128 or uint128 instead of ulong from
            // https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0 https://learn.microsoft.com/en-us/dotnet/api/system.uint128?view=net-7.0
            var resPile = ResPileInternal.CreateByMagic(amount: amount);
            return new
            (
                resPileInternal: resPile,
                thermalBody: ThermalBody.CreateByMagic(locationCounters: resPile.LocationCounters, amount: amount)
            );
        }

        public ResAmounts Amount
            => resPileInternal.Amount;

        public bool IsEmpty
            => Amount == ResAmounts.Empty;

        private readonly ResPileInternal resPileInternal;
        private ThermalBody thermalBody;

        private ResPile(ResPileInternal resPileInternal, ThermalBody thermalBody)
        {
            this.resPileInternal = resPileInternal;
            this.thermalBody = thermalBody;
        }

        public void ChangeLocation(ThermalBody newThermalBody)
        {
            newThermalBody.TransferResFrom(source: thermalBody, amount: Amount);
            resPileInternal.ChangeLocation(newLocationCounters: newThermalBody.locationCounters);
            thermalBody = newThermalBody;
        }

        public void TransferFrom(ResPile source, ResAmounts amount)
        {
            // This must be done first to get accurate source heat capacity in the calculations
            thermalBody.TransferResFrom(source: source.thermalBody, amount: amount);
            resPileInternal.TransferFrom(source: source.resPileInternal, amount: amount);
        }

        public void TransferAtMostFrom(ResPile source, ResAmounts maxAmount)
            => TransferFrom(source: source, amount: MyMathHelper.Min(maxAmount, Amount));

        public void TransferAllFrom(ResPile source)
            => TransferFrom(source: source, amount: source.Amount);

        public void TransferAllSingleResFrom(ResPile source, ResInd resInd)
            => TransferFrom
            (
                source: source,
                amount: new
                (
                    resInd: resInd,
                    amount: source.Amount[resInd]
                )
            );

        public void TransformFrom(ResPile source, ResRecipe recipe)
        {
            thermalBody.TransformResFrom(source: source.thermalBody, recipe: recipe);
            resPileInternal.TransformFrom(source: source.resPileInternal, recipe: recipe);
        }

        public void TransformTo<TDestinAmount>(EnergyPile<TDestinAmount> destin, ResAmounts amount)
            where TDestinAmount : struct, IUnconstrainedEnergy<TDestinAmount>
        {
            // Should I also transform an appropriate proportion of heat to TDestinAmount?
            throw new NotImplementedException();
            thermalBody.TransformResToEnergy(amount: amount);
            resPileInternal.TransformTo(destin: destin, amount: amount);
        }
    }
}
