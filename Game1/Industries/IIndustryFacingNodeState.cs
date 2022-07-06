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
        public ResPile StoredResPile { get; }
        public RealPeople WaitingPeople { get; }
        public BasicResInd ConsistsOfResInd { get; }
        public BasicRes ConsistsOfRes { get; }
        public bool TooManyResStored { get; }
        public UDouble WattsHittingSurfaceOrIndustry { get; }
        public MassCounter MassCounter { get; }

        public bool CanRemove(ulong resAmount);

        public void MineTo(ResPile destin, ulong resAmount);

        public void EnlargeFrom(ResPile source, ulong resAmount);
    }
}
