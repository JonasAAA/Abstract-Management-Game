using Game1.Inhabitants;

namespace Game1.Industries
{
    public interface IIndustryFacingNodeState : INodeShapeParams
    {
        //public NodeID NodeID { get; }
        //public Area Area { get; }
        //public UDouble radius { get; }
        //public UDouble SurfaceLength { get; }
        ////public ulong MaxAvailableResAmount { get; }
        //public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public ResPile StoredResPile { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public RealPeople WaitingPeople { get; }
        public RawMatAmounts Composition { get; }
        //public bool TooManyResStored { get; }
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        //public UDouble SurfaceGravity { get; }
        //public Temperature Temperature { get; }

        public Result<ResPile, TextErrors> Mine(AreaDouble targetArea, RawMatAllocator rawMatAllocator);

        public void EnlargeFrom(ResPile source, RawMatAmounts amount);
    }
}
