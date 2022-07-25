namespace Game1.Resources
{
    [Serializable]
    public sealed class ResPile : ResPileBase
    {
        public static ResPile CreateEmpty(MassCounter locationMassCounter)
            => new(locationMassCounter: locationMassCounter, createdByMagic: false);

        public static ResPile CreateFromSource(ResPile sourceResPile)
        {
            ResPile resPile = CreateEmpty(locationMassCounter: sourceResPile.LocationMassCounter);
            resPile.TransferAllFrom(source: sourceResPile);
            return resPile;
        }

        public static ResPile CreateMagicUnlimitedPile()
            // It's OK to create locationMassCounter here as this magic res pile is the only place where the "magic location mass counter" is used
            => new(locationMassCounter: MassCounter.CreateMassCounterByMagic(mass : magicResPileStartingResAmounts.TotalMass()), createdByMagic: true);

        private ResPile(MassCounter locationMassCounter, bool createdByMagic)
            : base(locationMassCounter: locationMassCounter, createdByMagic: createdByMagic)
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
