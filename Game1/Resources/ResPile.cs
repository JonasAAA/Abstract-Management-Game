using System.Diagnostics.CodeAnalysis;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResPile : ResPileBase
    {
        public static ResPile CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters, createdByMagic: false);

        public static ResPile CreateFromSource(ResPile sourceResPile)
        {
            ResPile resPile = CreateEmpty(locationCounters: sourceResPile.LocationCounters);
            resPile.TransferAllFrom(source: sourceResPile);
            return resPile;
        }

        public static ResPile CreateMagicUnlimitedPile()
            // It's OK to create locationMassCounter here as this magic res pile is the only place where the "magic location mass counter" is used
            => new
            (
                locationCounters: LocationCounters.CreateMassByMagic(mass: magicResPileStartingResAmounts.TotalMass()),
                createdByMagic: true
            );

        private ResPile(LocationCounters locationCounters, bool createdByMagic)
            : base(locationCounters: locationCounters, createdByMagic: createdByMagic)
        { }

        /// <summary>
        /// Transfers from source to this the min(source.ResAmounts, resAmounts)
        /// </summary>
        public void TransferAtMostFrom(ResPile source, ResAmounts resAmounts)
            => Transfer
            (
                source: source,
                destin: this,
                resAmounts: MyMathHelper.Min(source.ResAmounts, resAmounts)
            );

        ///// <summary>
        ///// Transfers from this to destin the min(this.ResAmounts, resAmounts)
        ///// </summary>
        //public void TransferUpTo(ResPile destin, ResAmounts resAmounts)
        //    => Transfer
        //    (
        //        source: this,
        //        destin: destin,
        //        resAmounts: MyMathHelper.Min(ResAmounts, resAmounts)
        //    );

        public void TransferAllFrom(ResPile source)
            => Transfer(source: source, destin: this, resAmounts: source.ResAmounts);

        public void TransferAllFrom([DisallowNull] ref ReservedResPile? reservedSource)
        {
            Transfer(source: reservedSource, destin: this, resAmounts: reservedSource.ResAmounts);
            reservedSource = null;
        }

        public void TransformAndTransferAllFrom([DisallowNull] ref IngredientsResPile? ingredients)
        {
            Transform(resPileBase: ingredients, recipe: ingredients.recipe);
            TransferAllFrom(source: ingredients, destin: this);
            ingredients = null;
        }

        //public void TransferAllTo(ResPile destin)
        //    => Transfer(source: this, destin: destin, resAmounts: ResAmounts);

        public void TransferAllSingleResFrom(ResPile source, ResInd resInd)
            => Transfer
            (
                source: source,
                destin: this,
                resAmount: new
                (
                    resInd: resInd,
                    amount: source[resInd]
                )
            );

        //public void TransferAllSingleResTo(ResPile destin, ResInd resInd)
        //    => Transfer
        //    (
        //        source: this,
        //        destin: destin,
        //        resAmount: new
        //        (
        //            resInd: resInd,
        //            amount: this[resInd]
        //        )
        //   );

        public override string ToString()
            => ResAmounts.ToString();
    }
}
