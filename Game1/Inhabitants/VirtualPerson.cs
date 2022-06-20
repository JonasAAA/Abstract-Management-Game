using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class VirtualPerson
    {
        public IReadOnlyDictionary<IndustryType, Score> enjoyments
            => realPerson.enjoyments;
        public IReadOnlyDictionary<IndustryType, Score> talents
            => realPerson.talents;
        public IReadOnlyDictionary<IndustryType, Score> skills
            => realPerson.skills;

        public NodeID ClosestNodeID
            => realPerson.ClosestNodeID;

        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => realPerson.LastActivityTimes;
        public UDouble reqWatts
            => realPerson.reqWatts;
        public TimeSpan seekChangeTime
            => realPerson.seekChangeTime;

        private readonly RealPerson realPerson;

        public VirtualPerson(RealPerson realPerson)
            => this.realPerson = realPerson;

        public bool IfSeeksNewActivity()
            => realPerson.IfSeeksNewActivity();

        public IPersonFacingActivityCenter ChooseActivityCenter(IEnumerable<IPersonFacingActivityCenter> activityCenters)
            => realPerson.ChooseActivityCenter(activityCenters: activityCenters);

        public void LetGoFromActivityCenter()
            => realPerson.LetGoFromActivityCenter();
    }
}
