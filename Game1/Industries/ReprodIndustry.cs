using Game1.ChangingValues;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class ReprodIndustry : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public readonly UDouble reqWattsPerChild;
            public readonly ulong maxCouplesPerUnitSurface;
            public readonly ResAmounts resPerChild;
            public readonly TimeSpan birthDuration;

            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerChild, ulong maxCouplesPerUnitSurface, ResAmounts resPerChild, TimeSpan birthDuration)
                : base
                (
                    industryType: IndustryType.Reproduction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerChild)} {reqWattsPerChild}\n{nameof(maxCouplesPerUnitSurface)} {maxCouplesPerUnitSurface}\n{nameof(resPerChild)} {resPerChild}\n{nameof(birthDuration)} {birthDuration.TotalSeconds:0.#} s"
                )
            {
                this.reqWattsPerChild = reqWattsPerChild;
                this.maxCouplesPerUnitSurface = maxCouplesPerUnitSurface;
                this.resPerChild = resPerChild;
                this.birthDuration = birthDuration;
            }

            public override bool CanCreateWith(NodeState state)
                => true;

            protected override ReprodIndustry InternalCreateIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        [Serializable]
        private class ReprodCenter : ActivityCenter
        {
            public readonly Queue<Person> unpairedPeople;

            private readonly IReadOnlyChangingULong maxCouples;

            public ReprodCenter(NodeState state, EnergyPriority energyPriority, IReadOnlyChangingULong maxCouples)
                : base(activityType: ActivityType.Reproduction, energyPriority: energyPriority, state: state)
            {
                this.maxCouples = maxCouples;
                unpairedPeople = new();
            }
            
            public override bool IsFull()
                => (ulong)allPeople.Count >= 2 * maxCouples.Value;

            public override bool IsPersonSuitable(Person person)
                // could disalow far travel
                => true;

            public override Score PersonScoreOfThis(Person person)
                => Score.WeightedAverage
                (
                    (
                        weight: 9,
                        // TODO: get rid of hard-coded constant
                        score: Score.FromUnboundedUDouble
                        (
                            value: (UDouble)(CurWorldManager.CurTime - person.LastActivityTimes[ActivityType]).TotalSeconds,
                            valueGettingAverageScore: 100
                        )
                    ),
                    (weight: 1, score: DistanceToHere(person: person))
                );

            public override void TakePerson(Person person)
            {
                base.TakePerson(person);
                unpairedPeople.Enqueue(person);
            }

            public override void UpdatePerson(Person person)
                => IActivityCenter.UpdatePersonDefault(person: person);

            public override bool CanPersonLeave(Person person)
                // a person can't leave while in the process of having a child
                => false;

            public string GetInfo()
                => $"{unpairedPeople.Count} waiting people\n{allPeople.Count - peopleHere.Count} people travelling here\n";
        }

        public override IEnumerable<Person> PeopleHere
            => base.PeopleHere.Concat(reprodCenter.PeopleHere);

        private readonly Params parameters;
        private readonly IReadOnlyChangingULong maxCouples;
        private readonly ReprodCenter reprodCenter;
        private readonly TimedQueue<(Person, Person)> birthQueue;

        public ReprodIndustry(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            maxCouples = state.approxSurfaceLength * parameters.maxCouplesPerUnitSurface;
            reprodCenter = new
            (
                state: state,
                energyPriority: parameters.energyPriority,
                maxCouples: maxCouples
            );

            birthQueue = new(duration: parameters.birthDuration);
        }

        public override ResAmounts TargetStoredResAmounts()
            => maxCouples.Value * parameters.resPerChild * state.maxBatchDemResStored;

        protected override bool IsBusy()
            => birthQueue.Count > 0;

        protected override ReprodIndustry InternalUpdate(Propor workingPropor)
        {
            birthQueue.Update(workingPropor: workingPropor);

            foreach (var (person1, person2) in birthQueue.DoneElements())
            {
                var newPerson = Person.GenerateChild(nodePos: state.position, person1: person1, person2: person2);
                state.waitingPeople.Add(newPerson);

                reprodCenter.RemovePerson(person: person1);
                reprodCenter.RemovePerson(person: person2);
            }

            while (reprodCenter.unpairedPeople.Count >= 2 && state.storedRes >= parameters.resPerChild)
            {
                Person person1 = reprodCenter.unpairedPeople.Dequeue(),
                    person2 = reprodCenter.unpairedPeople.Dequeue();
                birthQueue.Enqueue((person1, person2));
                state.storedRes -= parameters.resPerChild;
            }

            return this;
        }

        protected override void Delete()
        {
            base.Delete();
            reprodCenter.Delete();
            // need to disalow straight-up deletion.
            // first, births should finish, then people should evacuate, then can delete
            throw new NotImplementedException();
        }

        public override UDouble ReqWatts()
            => (UDouble)birthQueue.Count * parameters.reqWattsPerChild * CurSkillPropor;

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (CurWorldManager.Overlay is IPeopleOverlay)
            {
                text += $"{birthQueue.Count} children are being born\n";
                text += reprodCenter.GetInfo();
            }
            return text;
        }
    }
}
