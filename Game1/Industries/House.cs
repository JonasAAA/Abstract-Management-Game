using Game1.ChangingValues;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class House : Industry
    {
        [Serializable]
        public new class Params : Industry.Params
        {
            public readonly UDouble floorSpacePerUnitSurface;

            public Params(string name, UDouble floorSpacePerUnitSurface)
                : base
                (
                    name: name,
                    explanation: $"{nameof(floorSpacePerUnitSurface)} {floorSpacePerUnitSurface}"
                )
            {
                this.floorSpacePerUnitSurface = floorSpacePerUnitSurface;
            }

            public override House MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        [Serializable]
        private class Housing : ActivityCenter
        {
            private readonly Params parameters;
            private readonly IReadOnlyChangingUDouble floorSpace;

            public Housing(NodeState state, Params parameters)
                : base(activityType: ActivityType.Unemployed, energyPriority: EnergyPriority.maximal, state: state)
            {
                this.parameters = parameters;
                floorSpace = parameters.floorSpacePerUnitSurface * state.approxSurfaceLength;
            }
            
            public override bool IsFull()
                => false;

            public Score PersonalSpace()
                => PersonalSpace(peopleCount: peopleHere.Count);

            private Score PersonalSpace(ulong peopleCount)
                // TODO: get rid of hard-coded constant
                => Score.FromUnboundedUDouble(value: floorSpace.Value / peopleCount, valueGettingAverageScore: 10);

            public override Score PersonScoreOfThis(Person person)
                => Score.WightedAverageOfTwo
                (
                    score1: (IsPersonHere(person: person) ? Score.highest : Score.lowest),
                    // TODO: get rid of hard-coded constants
                    score2: Score.WeightedAverage
                    (
                        (weight: 7, score: PersonalSpace(peopleCount: allPeople.Count + 1)),
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

        public override IEnumerable<Person> PeopleHere
            => housing.PeopleHere;

        private readonly Params parameters;
        private readonly Housing housing;

        public House(NodeState state, Params parameters)
            : base(state: state)
        {
            this.parameters = parameters;
            housing = new(state: state, parameters: parameters);
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override House InternalUpdate()
            => this;

        public override string GetInfo()
            => $"{PeopleHere.Count()} people live here,\neach get {housing.PersonalSpace():#.##} floor space\n";
    }
}
