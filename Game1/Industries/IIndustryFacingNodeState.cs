using Game1.Inhabitants;

namespace Game1.Industries
{
    public interface IIndustryFacingNodeState : INodeShapeParams
    {
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public RealPeople WaitingPeople { get; }
        public RawMatAmounts Composition { get; }
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }

        public Result<ResPile, TextErrors> Mine(AreaDouble targetArea, RawMatAllocator rawMatAllocator);

        public void EnlargeFrom(ResPile source, RawMatAmounts amount);

        public void TransportRes(ResPile source, NodeID destination, AllResAmounts amount);
    }
}
