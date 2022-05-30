namespace Game1.Resources
{
    [Serializable]
    public sealed class ResPile : ResPileBase
    {
        public static ResPile Create(ResPile source)
        {
            ResPile resPile = CreateEmpty();
            source.TransferAllTo(destin: resPile);
            return resPile;
        }

        public static ResPile CreateEmpty()
            => new(createdByMagic: false);

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

        // TODO: delete if unused
        //public static void TransferAll(ResPile source, ResPile destin)
        //    => Transfer(source: source, destin: destin, resAmounts: source.ResAmounts);

        //public static void Transfer(ResPile source, ResPile destin, ResAmounts resAmounts)
        //{
        //    if (source == destin)
        //        throw new ArgumentException();
            
        //    source.ResAmounts -= resAmounts;
        //    destin.ResAmounts += resAmounts;
        //}

        // TODO: this method is O(resCount) though could probably be optimised into O(1)
        //public static void Transfer(ResPile source, ResPile destin, ResAmount resAmount)
        //    => Transfer
        //    (
        //        source: source,
        //        destin: destin,
        //        resAmounts: new ResAmounts(resAmount: resAmount)
        //    );

        //public void Transform(ResRecipe resRecipe)
        //{
        //    ResAmounts -= resRecipe.ingredients;
        //    ResAmounts += resRecipe.results;
        //}

        //public void TransformAll(ResRecipe resRecipe)
        //{
        //    if (ResAmounts != resRecipe.ingredients)
        //        throw new ArgumentException();
        //    Transform(resRecipe: resRecipe);
        //}

        public override string ToString()
            => ResAmounts.ToString();
    }
}
