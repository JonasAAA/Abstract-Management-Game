namespace Game1.Resources
{
    [Serializable]
    public abstract class ResPileBase : IMyArray<ulong>
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
                Mass = resAmounts.Mass();
                heatCapacity = resAmounts.HeatCapacity();
            }
        }
        public LocationCounters LocationCounters
        {
            get => locationCounters;
            set
            {
                value.TransferResFrom(source: locationCounters, resAmounts: ResAmounts);
                locationCounters = value;
            }
        }
        private LocationCounters locationCounters;
        /// <summary>
        /// NEVER use directly
        /// </summary>
        private ResAmounts resAmounts;
        private HeatCapacity heatCapacity;
#if DEBUG2
        private readonly bool createdByMagic;
#endif


        protected ResPileBase(LocationCounters locationCounters, bool createdByMagic = false)
        {
            this.locationCounters = locationCounters;
            ResAmounts = createdByMagic ? magicResPileStartingResAmounts : ResAmounts.Empty;
#if DEBUG2
            this.createdByMagic = createdByMagic;
#endif
        }

        public ulong this[ResInd resInd]
            => ResAmounts[resInd];

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmounts resAmounts)
        {
            if (source == destin)
                throw new ArgumentException();

            source.ResAmounts -= resAmounts;
            destin.ResAmounts += resAmounts;
            destin.LocationCounters.TransferResFrom(source: source.locationCounters, resAmounts: resAmounts);
        }

        protected static void Transfer(ResPileBase source, ResPileBase destin, ResAmount resAmount)
            => Transfer(source: source, destin: destin, resAmounts: new(resAmount: resAmount));

        protected static void TransferAllFrom(ResPileBase source, ResPileBase destin)
            => Transfer(source: source, destin: destin, resAmounts: source.ResAmounts);

        protected static void Transform(ResPileBase resPileBase, ResRecipe recipe)
        {
            resPileBase.ResAmounts -= recipe.ingredients;
            resPileBase.ResAmounts += recipe.results;
        }

#if DEBUG2
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
