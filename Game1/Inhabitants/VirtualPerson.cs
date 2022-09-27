using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class VirtualPerson
    {
        public EnumDict<IndustryType, Score> Enjoyments
            => realPerson.RealPeopleStats.enjoyments;
        public EnumDict<IndustryType, Score> Talents
            => realPerson.RealPeopleStats.talents;
        public EnumDict<IndustryType, Score> Skills
            => realPerson.RealPeopleStats.skills;
        public NodeID ClosestNodeID
            => realPerson.ClosestNodeID;
        public EnumDict<ActivityType, TimeSpan> LastActivityTimes
            => realPerson.LastActivityTimes;
        public UDouble ReqWatts
            => realPerson.reqWatts;
        public TimeSpan SeekChangeTime
            => realPerson.seekChangeTime;
        public Score Happiness
            => realPerson.RealPeopleStats.happiness;
        public TimeSpan Age
            => realPerson.RealPeopleStats.age;

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
