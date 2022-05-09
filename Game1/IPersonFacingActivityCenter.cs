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
        public Score PersonScoreOfThis(Person person);

        public bool IsPersonHere(Person person);

        public void TakePerson(Person person);

        public void UpdatePerson(Person person);

        public bool CanPersonLeave(Person person);
        
        public void RemovePerson(Person person);
    }
}
