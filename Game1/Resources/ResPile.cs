namespace Game1.Resources
{
    [Serializable]
    public sealed class ResPile : IMyArray<ulong>
    {
        public static ResPile? Create(ResPile source, ResAmounts resAmounts)
        {
            if (source.ResAmounts >= resAmounts)
            {
                ResPile resPile = CreateEmpty();
                Transfer(source: source, destin: resPile, resAmounts: resAmounts);
                return resPile;
            }
            return null;
        }

        public static ResPile Create(ResPile source)
        {
            ResPile resPile = CreateEmpty();
            TransferAll(source: source, destin: resPile);
            return resPile;
        }

        public static ResPile CreateEmpty()
            => new(createdByMagic: false);

        public static ResPile CreateMagicUnlimitedPile()
            => new(createdByMagic: true)
            {
                ResAmounts = new ResAmounts(value: uint.MaxValue)
            };

        public ulong TotalMass { get; private set; }
        public bool IsEmpty
            => TotalMass is 0;
        public ResAmounts ResAmounts
        {
            get => resAmounts;
            private set
            {
                resAmounts = value;
                TotalMass = resAmounts.TotalMass();
            }
        }

        /// <summary>
        /// NEVER use directly
        /// </summary>
        private ResAmounts resAmounts;
        private readonly bool createdByMagic;

        private ResPile(bool createdByMagic)
        {
            ResAmounts = new();
            this.createdByMagic = createdByMagic;
        }

        public ulong this[ResInd resInd]
            => ResAmounts[resInd];

        public static void TransferAll(ResPile source, ResPile destin)
            => Transfer(source: source, destin: destin, resAmounts: source.ResAmounts);

        public static void Transfer(ResPile source, ResPile destin, ResAmounts resAmounts)
        {
            if (source == destin)
                throw new ArgumentException();
            
            source.ResAmounts -= resAmounts;
            destin.ResAmounts += resAmounts;
        }

        // TODO: this method is O(resCount) though could probably be optimised into O(1)
        public static void Transfer(ResPile source, ResPile destin, ResAmount resAmount)
            => Transfer
            (
                source: source,
                destin: destin,
                resAmounts: new ResAmounts(resAmount: resAmount)
            );

        public void Transform(ResRecipe resRecipe)
        {
            ResAmounts -= resRecipe.ingredients;
            ResAmounts += resRecipe.results;
        }

        public void TransformAll(ResRecipe resRecipe)
        {
            if (ResAmounts != resRecipe.ingredients)
                throw new ArgumentException();
            Transform(resRecipe: resRecipe);
        }

        public override string ToString()
            => ResAmounts.ToString();

#if DEBUG
        ~ResPile()
        {
            if (createdByMagic)
                return;
            if (!IsEmpty || !ResAmounts.IsEmpty())
                throw new Exception($"can only discard empty {nameof(ResPile)}");
        }
#endif
    }
}
