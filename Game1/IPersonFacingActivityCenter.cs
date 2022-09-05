using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1
{
    public interface IPersonFacingActivityCenter
    {
        public ActivityType ActivityType { get; }

        public NodeID NodeID { get; }

        public EnergyPriority EnergyPriority { get; }

        // TODO: Make sure that repeated measurements actually give the same score/rethink if that should be the case
        /// <summary>
        /// can include some randomness, but repeated measurements should give the same score
        /// gives higher/lower score to the current place of the person depending on
        /// if person recently got queued
        /// </summary>
        public Score PersonEnjoymentOfThis(VirtualPerson person);

        public bool IsPersonHere(VirtualPerson person);

        public void TakePersonFrom(RealPeople realPersonSource, RealPerson realPerson);

        public bool CanPersonLeave(VirtualPerson person);

        public void RemovePerson(VirtualPerson person, bool force = false);

        /// <summary>
        /// Used this to calculate personal score
        /// </summary>
        public sealed Score DistanceToHereAsPerson(VirtualPerson person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.PersonDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();

        /// <summary>
        /// Used this to calculate suitability of person
        /// </summary>
        protected sealed Score DistanceToHereAsRes(VirtualPerson person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.ResDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();
    }
}
