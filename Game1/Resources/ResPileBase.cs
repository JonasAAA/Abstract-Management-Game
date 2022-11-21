﻿namespace Game1.Resources
{
    [Serializable]
    public abstract class ResPileBase : IMyArray<ulong>
    {
        protected static readonly ResAmounts magicResPileStartingResAmounts;

        static ResPileBase()
            => magicResPileStartingResAmounts = new(value: uint.MaxValue);

        public Mass Mass { get; private set; }
        public bool IsEmpty
            => Mass.isZero;

        public ResAmounts ResAmounts
        {
            get => resAmounts;
            private set
            {
                resAmounts = value;
                Mass = resAmounts.TotalMass();
            }
        }
        public LocationCounters LocationCounters
        {
            get => locationCounters;
            set
            {
                value.TransferFrom(source: locationCounters, mass: Mass, numPeople: NumPeople.zero);
                locationCounters = value;
            }
        }
        private LocationCounters locationCounters;
        /// <summary>
        /// NEVER use directly
        /// </summary>
        private ResAmounts resAmounts;
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
            destin.LocationCounters.TransferFrom(source: source.locationCounters, mass: resAmounts.TotalMass(), numPeople: NumPeople.zero);
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
