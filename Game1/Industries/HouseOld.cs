using Game1.PrimitiveTypeWrappers;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class HouseOld : ActivityCenter
    {
        public HouseOld(NodeState state)
            : base(activityType: ActivityType.Unemployed, energyPriority: EnergyPriority.maximal, state: state)
        { }

        public override bool IsFull()
            => false;

        public override Score PersonScoreOfThis(Person person)
            => Score.CombineTwo
            (
                score1: (IsPersonHere(person: person) ? Score.best : Score.worst),
                // TODO: get rid of hard-coded constants
                score2: Score.Combine
                (
                    // TODO: make it so that multiple samples generate the same value
                    (weight: 7, score: Score.GenerateRandom()),
                    (weight: 3, score: DistanceToHere(person: person))
                ),
                score1Propor: CurWorldConfig.personMomentumPropor
            );

        public override bool IsPersonSuitable(Person person)
            // may disallow far travel
            => true;

        public override void UpdatePerson(Person person)
        {
            if (!IsPersonHere(person: person))
                throw new ArgumentException();

            IActivityCenter.UpdatePersonDefault(person: person);
            // TODO calculate happiness
            // may decrease person's skills
        }

        public override bool CanPersonLeave(Person person)
            => true;

        public string GetInfo()
            => $"unemployed {peopleHere.Count}\ntravel to be unemployed\nhere {allPeople.Count - peopleHere.Count}\n";
    }
}
