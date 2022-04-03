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

        public override double PersonScoreOfThis(Person person)
            => CurWorldConfig.personMomentumCoeff * (IsPersonHere(person: person) ? 1 : 0)
            + (.7 * C.Random(min: 0, max: 1) + .3 * DistanceToHere(person: person)) * (1 - CurWorldConfig.personMomentumCoeff);

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
