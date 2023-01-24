using Game1.Inhabitants;

namespace Game1.Industries
{
    public interface IIndustryFacingNodeState
    {
        public NodeID NodeID { get; }
        public UDouble Radius { get; }
        public ulong ApproxSurfaceLength { get; }
        public ulong MaxAvailableResAmount { get; }
        public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public Pile<ResAmounts> StoredResPile { get; }
        public RealPeople WaitingPeople { get; }
        public BasicResInd ConsistsOfResInd { get; }
        public BasicRes ConsistsOfRes { get; }
        public bool TooManyResStored { get; }
        public LocationCounters LocationCounters { get; }

        public bool CanRemove(ulong resAmount);

        public void MineTo<TDestinPile>(TDestinPile destin, ulong resAmount)
            where TDestinPile : IDestinPile<ResAmounts>;

        public void EnlargeFrom<TSourcePile>(TSourcePile source, ulong resAmount)
            where TSourcePile : ISourcePile<ResAmounts>;
    }
}
