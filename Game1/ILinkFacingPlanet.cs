using Game1.Inhabitants;

namespace Game1
{
    public interface ILinkFacingPlanet
    {
        public MyVector2 Position { get; }

        public NodeID NodeID { get; }

        public UDouble SurfaceGravity { get; }

        public void AddLink(Link link);

        public void Arrive(ResAmountsPacketsByDestin resAmountsPackets);

        public void Arrive(RealPeople realPeople);

        public void Arrive(RealPerson realPerson, RealPeople realPersonSource);
    }
}
