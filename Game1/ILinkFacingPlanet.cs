using Game1.Inhabitants;
using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    public interface ILinkFacingPlanet
    {
        public MyVector2 Position { get; }

        public NodeID NodeID { get; }

        public void AddLink(Link link);

        // TODO: continue implementing these
        //public sealed void TransferFrom(Link link, [DisallowNull] ResAmountsPacketsByDestin? resAmountsPackets)
        //{
        //    TransferFrom(link: link, hasMass: resAmountsPackets);
        //    Arrive(resAmountsPackets: resAmountsPackets);
        //}

        //public sealed void TransferFrom(Link link, [DisallowNull] ResAmountsPacketsByDestin? resAmountsPackets)
        //{
        //    TransferFrom(link: link, hasMass: resAmountsPackets);
        //    Arrive(resAmountsPackets: resAmountsPackets);
        //}

        //public sealed void TransferFrom(Link link, RealPeople people)
        //{
        //    TransferFrom(link: link, hasMass: people);
        //    Arrive(people: people);
        //}

        //public sealed void TransferFrom(Link link, RealPerson person)
        //{
        //    TransferFrom(link: link, hasMass: person);
        //    Arrive(person: person);
        //}

        public void Arrive([DisallowNull] ResAmountsPacketsByDestin? resAmountsPackets);

        public void Arrive(RealPeople people);

        public void Arrive(RealPerson person, RealPeople personSource);

        // TODO: continue implementing these
        //private void TransferFrom(Link link, IHasMass hasMass)
        //{
        //    AddMass(mass: hasMass.Mass);
        //    link.RemoveMass(mass: hasMass.Mass);
        //}

        //protected void AddMass(ulong mass);
    }
}
