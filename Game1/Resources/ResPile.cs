using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public class ResPile
    {
        [Serializable]
        private sealed class ResPileInternal : EnergyPile<AllResAmounts>
        {
            public new static ResPileInternal CreateEmpty(LocationCounters locationCounters)
                => new(locationCounters: locationCounters, counter: ResCounter.CreateEmpty());

            public static ResPileInternal? CreateIfHaveEnough(ResPileInternal source, AllResAmounts amount)
            {
                if (source.Amount >= amount)
                {
                    ResPileInternal newPile = new(locationCounters: source.LocationCounters, counter: ResCounter.CreateEmpty());
                    newPile.TransferFrom(source: source, amount: amount);
                    return newPile;
                }
                return null;
            }

            public new static ResPileInternal CreateByMagic(AllResAmounts amount)
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
            {
                Counter.TransformFrom(source: source.Counter, recipe: recipe);
                LocationCounters.TransformFrom(source: source.LocationCounters, recipe: recipe);
            }

            public void TransformTo(ResPileInternal destin, ResRecipe recipe)
            {
                Counter.TransformTo(destin: destin.Counter, recipe: recipe);
                LocationCounters.TransformTo(destin: destin.LocationCounters, recipe: recipe);
            }
        }

        public static ResPile CreateEmpty(ThermalBody thermalBody)
            => new
            (
                resPileInternal: ResPileInternal.CreateEmpty(locationCounters: thermalBody.locationCounters),
                thermalBody: thermalBody
            );

        public static ResPile? CreateIfHaveEnough(ResPile source, SomeResAmounts<IResource> amount)
            => CreateIfHaveEnough(source: source, amount: AllResAmounts.CreateFromNoMix(resAmounts: amount));

        public static ResPile? CreateIfHaveEnough(ResPile source, RawMaterialsMix amount)
            => CreateIfHaveEnough(source: source, amount: AllResAmounts.CreateFromOnlyMix(rawMatsMix: amount));

        //public static ResPile? CreateIfHaveEnough(ResPile source, AllResAmounts amount)
        //{
        //    var newPile = ResPileInternal.CreateIfHaveEnough(source: source.resPileInternal, amount: amount);
        //    if (newPile is null)
        //        return null;
        //    return new(resPileInternal: newPile, thermalBody: source.thermalBody);
        //}

        private static ResPile? CreateIfHaveEnough(ResPile source, AllResAmounts amount)
        {
            var newPile = ResPileInternal.CreateIfHaveEnough(source: source.resPileInternal, amount: amount);
            if (newPile is null)
                return null;
            return new(resPileInternal: newPile, thermalBody: source.thermalBody);
        }

        public static ResPile CreateByMagic(AllResAmounts amount)
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

        public AllResAmounts Amount
            => resPileInternal.Amount;

        public bool IsEmpty
            => Amount.IsEmpty;

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

        public void TransferFrom(ResPile source, AllResAmounts amount)
        {
            // This must be done first to get accurate source heat capacity in the calculations
            thermalBody.TransferResFrom(source: source.thermalBody, amount: amount);
            resPileInternal.TransferFrom(source: source.resPileInternal, amount: amount);
        }

        public void TransferAtMostFrom(ResPile source, AllResAmounts maxAmount)
            => TransferFrom(source: source, amount: MyMathHelper.Min(maxAmount, Amount));

        public void TransferAllFrom(ResPile source)
            => TransferFrom(source: source, amount: source.Amount);

        public void TransferAllSingleResFrom(ResPile source, IResource res)
            => TransferFrom
            (
                source: source,
                amount: AllResAmounts.CreateFromNoMix
                (
                    resAmounts: new(res: res, amount: source.Amount.resAmounts[res])
                )
            );

        public void TransformFrom(ResPile source, ResRecipe recipe)
        {
            thermalBody.TransformResFrom(source: source.thermalBody, recipe: recipe);
            resPileInternal.TransformFrom(source: source.resPileInternal, recipe: recipe);
        }

        public void TransformResToHeatEnergy(RawMaterialsMix rawMatsMix)
            => thermalBody.TransformResToHeatEnergy(source: resPileInternal, rawMatsMix: rawMatsMix);
    }
}
