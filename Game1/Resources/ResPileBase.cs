namespace Game1.Resources
{
    [Serializable]
    public abstract class ResPileBase : IMyArray<ulong>, IHasMass
    {
        public ulong Mass { get; private set; }
        public bool IsEmpty
            => Mass is 0;

        public ResAmounts ResAmounts
        {
            get => resAmounts;
            private set
            {
                resAmounts = value;
                Mass = resAmounts.TotalMass();
            }
        }

        /// <summary>
        /// NEVER use directly
        /// </summary>
        private ResAmounts resAmounts;
        private readonly bool createdByMagic;

        protected ResPileBase(bool createdByMagic = false)
        {
            this.createdByMagic = createdByMagic;
            ResAmounts = createdByMagic ? new(value: uint.MaxValue) : ResAmounts.Empty;
        }

        public ulong this[ResInd resInd]
            => ResAmounts[resInd];

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmounts resAmounts)
        {
            if (source == destin)
                throw new ArgumentException();

            source.ResAmounts -= resAmounts;
            destin.ResAmounts += resAmounts;
        }

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmount resAmount)
            => Transfer(source: source, destin: destin, resAmounts: new(resAmount: resAmount));

        protected static void TransferAll(ResPileBase source, ResPileBase destin)
            => Transfer(source: source, destin: destin, resAmounts: source.ResAmounts);

        protected void Transform(ResRecipe recipe)
        {
            ResAmounts -= recipe.ingredients;
            ResAmounts += recipe.results;
        }

#if DEBUG
        ~ResPileBase()
        {
            if (createdByMagic)
                return;
            if (!IsEmpty || !ResAmounts.IsEmpty())
                throw new Exception($"can only discard empty {nameof(ResPile)}");
        }
#endif
    }
}
