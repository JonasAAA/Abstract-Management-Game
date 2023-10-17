using Game1.Inhabitants;

namespace Game1
{
    public interface ILinkFacingCosmicBody
    {
        public MyVector2 Position { get; }

        public NodeID NodeID { get; }

        public SurfaceGravity SurfaceGravity { get; }

        public void AddLink(Link link);

        public void Arrive(ResAmountsPacketsByDestin resAmountsPackets);

        public void ArriveAndDeleteSource(RealPeople realPeopleSource);

        public void Arrive(RealPerson realPerson, RealPeople realPersonSource);

        public void TransformAllElectricityToHeatAndTransferFrom(EnergyPile<ElectricalEnergy> source);
    }
}
