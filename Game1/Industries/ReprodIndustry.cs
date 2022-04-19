using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class ReprodIndustry : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory
        {
            public readonly UDouble reqWattsPerChild;
            public readonly ulong maxCouplesPerUnitSurface;
            public readonly ResAmounts resPerChild;
            public readonly TimeSpan birthDuration;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerChild, ulong maxCouplesPerUnitSurface, ResAmounts resPerChild, TimeSpan birthDuration)
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

            public override ReprodIndustry CreateIndustry(NodeState state)
                => new(parameters: new(state: state, factory: this));
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public readonly UDouble reqWattsPerChild;
            public ulong MaxCouples
                => state.ApproxSurfaceLength * factory.maxCouplesPerUnitSurface;
            public readonly ResAmounts resPerChild;
            public readonly TimeSpan birthDuration;

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                reqWattsPerChild = factory.reqWattsPerChild;
                resPerChild = factory.resPerChild;
                birthDuration = factory.birthDuration;
            }
        }

        [Serializable]
        private class ReprodCenter : ActivityCenter
        {
            public readonly Queue<Person> unpairedPeople;

            private readonly Params parameters;

            public ReprodCenter(Params parameters)
                : base(activityType: ActivityType.Reproduction, energyPriority: parameters.energyPriority, state: parameters.state)
            {
                this.parameters = parameters;
                unpairedPeople = new();
            }
            
            public override bool IsFull()
                => allPeople.Count >= 2 * parameters.MaxCouples;

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
                    (weight: 1, score: DistanceToHereAsPerson(person: person))
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
        private readonly ReprodCenter reprodCenter;
        private readonly TimedQueue<(Person, Person)> birthQueue;

        public ReprodIndustry(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            reprodCenter = new(parameters: parameters);

            birthQueue = new(duration: parameters.birthDuration);
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.MaxCouples * parameters.resPerChild * parameters.state.maxBatchDemResStored;

        protected override bool IsBusy()
            => birthQueue.Count > 0;

        protected override ReprodIndustry InternalUpdate(Propor workingPropor)
        {
            birthQueue.Update(workingPropor: workingPropor);

            foreach (var (person1, person2) in birthQueue.DoneElements())
            {
                var newPerson = Person.GenerateChild(nodeId: parameters.state.nodeId, person1: person1, person2: person2);
                parameters.state.waitingPeople.Add(newPerson);

                reprodCenter.RemovePerson(person: person1);
                reprodCenter.RemovePerson(person: person2);
            }

            while (reprodCenter.unpairedPeople.Count >= 2 && parameters.state.storedRes >= parameters.resPerChild)
            {
                Person person1 = reprodCenter.unpairedPeople.Dequeue(),
                    person2 = reprodCenter.unpairedPeople.Dequeue();
                birthQueue.Enqueue((person1, person2));
                parameters.state.storedRes -= parameters.resPerChild;
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
