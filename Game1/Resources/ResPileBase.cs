namespace Game1.Resources
{
    [Serializable]
    public abstract class ResPileBase : IMyArray<ulong>, IHasMass
    {
        protected static readonly ResAmounts magicResPileStartingResAmounts;

        static ResPileBase()
            => magicResPileStartingResAmounts = new(value: uint.MaxValue);

        public Mass Mass { get; private set; }
        public bool IsEmpty
            => Mass.IsZero;

        public ResAmounts ResAmounts
        {
            get => resAmounts;
            private set
            {
                resAmounts = value;
                Mass = resAmounts.TotalMass();
            }
        }
        public MassCounter LocationMassCounter
        {
            get => locationMassCounter;
            set
            {
                value.TransferFrom(source: locationMassCounter, mass: Mass);
                locationMassCounter = value;
            }
        }

        private MassCounter locationMassCounter;
        /// <summary>
        /// NEVER use directly
        /// </summary>
        private ResAmounts resAmounts;
        private readonly bool createdByMagic;

        protected ResPileBase(MassCounter locationMassCounter, bool createdByMagic = false)
        {
            this.locationMassCounter = locationMassCounter;
            this.createdByMagic = createdByMagic;
            ResAmounts = createdByMagic ? magicResPileStartingResAmounts : ResAmounts.Empty;
        }

        public ulong this[ResInd resInd]
            => ResAmounts[resInd];

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmounts resAmounts)
        {
            if (source == destin)
                throw new ArgumentException();

            source.ResAmounts -= resAmounts;            
            destin.ResAmounts += resAmounts;
            destin.LocationMassCounter.TransferFrom(source: source.LocationMassCounter, mass: resAmounts.TotalMass());
        }

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmount resAmount)
            => Transfer(source: source, destin: destin, resAmounts: new(resAmount: resAmount));

        protected static void TransferAllFrom(ResPileBase source, ResPileBase destin)
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
