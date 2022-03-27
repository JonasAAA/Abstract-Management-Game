using Game1.PrimitiveTypeWrappers;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class House : Industry
    {
        [Serializable]
        public new class Params : Industry.Params
        {
            public readonly UFloat floorSpacePerUnitSurface;

            public Params(string name, UFloat floorSpacePerUnitSurface)
                : base
                (
                    name: name,
                    explanation: $"{nameof(floorSpacePerUnitSurface)} {floorSpacePerUnitSurface}"
                )
            {
                if (floorSpacePerUnitSurface < 0)
                    throw new ArgumentException();
                this.floorSpacePerUnitSurface = floorSpacePerUnitSurface;
            }

            public override House MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        [Serializable]
        private class Housing : ActivityCenter
        {
            private readonly Params parameters;
            private readonly IReadOnlyChangingUFloat floorSpace;

            public Housing(NodeState state, Params parameters)
                : base(activityType: ActivityType.Unemployed, energyPriority: ulong.MaxValue, state: state)
            {
                this.parameters = parameters;
                floorSpace = parameters.floorSpacePerUnitSurface * state.approxSurfaceLength;
            }
            
            public override bool IsFull()
                => false;

            public double PersonalSpace()
                => PersonalSpace(peopleCount: peopleHere.Count);

            private double PersonalSpace(int peopleCount)
                => Math.Tanh(floorSpace.Value / peopleCount);

            public override double PersonScoreOfThis(Person person)
                => CurWorldConfig.personMomentumCoeff * (IsPersonHere(person: person) ? 1 : 0)
                + (.7 * PersonalSpace(peopleCount: allPeople.Count + 1) + .3 * DistanceToHere(person: person)) * (1 - CurWorldConfig.personMomentumCoeff);

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

        public override ULongArray TargetStoredResAmounts()
            => new();

        protected override House InternalUpdate()
            => this;

        public override string GetInfo()
            => $"{PeopleHere.Count()} people live here,\neach get {housing.PersonalSpace():#.##} floor space\n";
    }
}
