using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class House : Industry
    {
        [Serializable]
        public new sealed class Factory : Industry.Factory
        {
            public readonly UDouble floorSpacePerUnitSurface;

            public Factory(string name, UDouble floorSpacePerUnitSurface)
                : base(name: name, color: Color.Yellow)
            {
                this.floorSpacePerUnitSurface = floorSpacePerUnitSurface;
            }

            public override House CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);
        }

        [Serializable]
        public new sealed class Params : Industry.Params
        {
            public UDouble FloorSpace
                => state.ApproxSurfaceLength * factory.floorSpacePerUnitSurface;

            public override string TooltipText
                => base.TooltipText + $"{nameof(FloorSpace)}: {FloorSpace}\n";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        [Serializable]
        private sealed class Housing : ActivityCenter
        {
            private readonly Params parameters;

            public Housing(Params parameters)
                : base(activityType: ActivityType.Unemployed, energyPriority: EnergyPriority.maximal, state: parameters.state)
            {
                this.parameters = parameters;
            }

            public override bool IsFull()
                => false;

            public Score PersonalSpace()
                => PersonalSpace(peopleCount: peopleHere.Count);

            private Score PersonalSpace(ulong peopleCount)
                // TODO: get rid of hard-coded constant
                => Score.FromUnboundedUDouble(value: parameters.FloorSpace / peopleCount, valueGettingAverageScore: 10);

            public override Score PersonScoreOfThis(Person person)
                => Score.WightedAverageOfTwo
                (
                    score1: (IsPersonHere(person: person) ? Score.highest : Score.lowest),
                    // TODO: get rid of hard-coded constants
                    score2: Score.WeightedAverage
                    (
                        (weight: 7, score: PersonalSpace(peopleCount: allPeople.Count + 1)),
                        (weight: 3, score: DistanceToHereAsPerson(person: person))
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

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;
        private readonly Housing housing;

        public House(Params parameters)
            : base(parameters: parameters)
        {
            housing = new(parameters: parameters);
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override House InternalUpdate()
            => this;

        public override string GetInfo()
            => $"{PeopleHere.Count()} people live here,\neach get {housing.PersonalSpace():#.##} floor space\n";
    }
}
