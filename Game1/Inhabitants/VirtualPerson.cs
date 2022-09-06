using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class VirtualPerson
    {
        public IReadOnlyDictionary<IndustryType, Score> Enjoyments
            => realPerson.enjoyments;
        public IReadOnlyDictionary<IndustryType, Score> Talents
            => realPerson.talents;
        public IReadOnlyDictionary<IndustryType, Score> Skills
            => realPerson.Skills;
        public NodeID ClosestNodeID
            => realPerson.ClosestNodeID;
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => realPerson.LastActivityTimes;
        public UDouble ReqWatts
            => realPerson.reqWatts;
        public TimeSpan SeekChangeTime
            => realPerson.seekChangeTime;
        public Score Happiness
            => realPerson.Happiness;
        public Score MomentaryHappiness
            => realPerson.MomentaryHappiness;

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
