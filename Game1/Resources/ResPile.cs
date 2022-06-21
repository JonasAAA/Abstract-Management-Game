namespace Game1.Resources
{
    [Serializable]
    public sealed class ResPile : ResPileBase
    {
        public static ResPile CreateEmpty()
            => new(createdByMagic: false);

        public static ResPile CreateFromSource(ResPile sourceResPile)
        {
            ResPile resPile = CreateEmpty();
            sourceResPile.TransferAllTo(destin: resPile);
            return resPile;
        }

        public static ResPile CreateMagicUnlimitedPile()
            => new(createdByMagic: true);

        private ResPile(bool createdByMagic)
            : base(createdByMagic: createdByMagic)
        { }

        /// <summary>
        /// Transfers from this to destin the min(this.ResAmounts, resAmounts)
        /// </summary>
        public void TransferUpTo(ResPile destin, ResAmounts resAmounts)
            => Transfer
            (
                source: this,
                destin: destin,
                resAmounts: MyMathHelper.Min(ResAmounts, resAmounts)
            );

        public void TransferAllTo(ResPile destin)
            => Transfer(source: this, destin: destin, resAmounts: ResAmounts);

        public void TransferAllSingleResTo(ResPile destin, ResInd resInd)
            => Transfer
            (
                source: this,
                destin: destin,
                resAmount: new
                (
                    resInd: resInd,
                    amount: this[resInd]
                )
           );

        public override string ToString()
            => ResAmounts.ToString();
    }
}
