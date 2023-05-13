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
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public RealPeople WaitingPeople { get; }
        public RawMaterial ConsistsOf { get; }
        public bool TooManyResStored { get; }
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }

        public bool CanRemove(ulong resAmount);

        public void MineTo(ResPile destin, ulong resAmount);

        public void EnlargeFrom(ResPile source, ulong resAmount);
    }
}
