using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class VirtualPerson
    {
        public EnumDict<IndustryType, Score> Enjoyments
            => realPerson.enjoyments;
        public EnumDict<IndustryType, Score> Talents
            => realPerson.talents;
        public EnumDict<IndustryType, Score> Skills
            => realPerson.Skills;
        public NodeID ClosestNodeID
            => realPerson.ClosestNodeID;
        public EnumDict<ActivityType, TimeSpan> LastActivityTimes
            => realPerson.LastActivityTimes;
        public UDouble ReqWatts
            => realPerson.reqWatts;
        public TimeSpan SeekChangeTime
            => realPerson.seekChangeTime;
        public Score Happiness
            => realPerson.RealPeopleStats.Happiness;
        public TimeSpan Age
            => realPerson.RealPeopleStats.Age;

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
