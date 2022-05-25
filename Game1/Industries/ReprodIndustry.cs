using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class ReprodIndustry : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
        {
            public readonly UDouble reqWattsPerChild;
            public readonly UDouble maxCouplesPerUnitSurface;
            public readonly TimeSpan birthDuration;
            private readonly ResAmounts buildingCostPerUnitSurface;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerChild, UDouble maxCouplesPerUnitSurface, TimeSpan birthDuration, ResAmounts buildingCostPerUnitSurface)
                : base
                (
                    industryType: IndustryType.Reproduction,
                    name: name,
                    color: Color.Green,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                this.reqWattsPerChild = reqWattsPerChild;
                this.maxCouplesPerUnitSurface = maxCouplesPerUnitSurface;
                if (birthDuration <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException();
                this.birthDuration = birthDuration;

                if (buildingCostPerUnitSurface.IsEmpty())
                    throw new ArgumentException();
                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
            }

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            public ResAmounts BuildingCost(NodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(NodeState state, Building building)
                => new ReprodIndustry(parameters: CreateParams(state: state), building: building);
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public ResAmounts BuildingCost
                => factory.BuildingCost(state: state);
            public readonly UDouble reqWattsPerChild;
            public ulong MaxCouples
                => (ulong)(state.ApproxSurfaceLength * factory.maxCouplesPerUnitSurface);
            public readonly TimeSpan birthDuration;
            public override string TooltipText
                => base.TooltipText + $"{nameof(BuildingCost)}: {BuildingCost}\n{nameof(reqWattsPerChild)}: {reqWattsPerChild}\n{nameof(MaxCouples)}: {MaxCouples}\nresPerChild: {Person.resAmountsPerPerson}\n{nameof(birthDuration)}: {birthDuration}";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                reqWattsPerChild = factory.reqWattsPerChild;
                birthDuration = factory.birthDuration;
            }
        }

        [Serializable]
        private sealed class ReprodCenter : ActivityCenter
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

        public override bool PeopleWorkOnTop
            => false;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;
        private readonly ReprodCenter reprodCenter;
        private readonly TimedQueue<(Person, Person, ResPile childResPile)> birthQueue;

        private ReprodIndustry(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            reprodCenter = new(parameters: parameters);

            birthQueue = new(duration: parameters.birthDuration);
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.MaxCouples * Person.resAmountsPerPerson * parameters.state.maxBatchDemResStored;

        protected override bool IsBusy()
            => birthQueue.Count > 0;

        protected override ReprodIndustry InternalUpdate(Propor workingPropor)
        {
            birthQueue.Update(workingPropor: workingPropor);

            foreach (var (person1, person2, childResPile) in birthQueue.DoneElements())
            {
                var newPerson = Person.GenerateChild
                (
                    nodeID: parameters.state.nodeID,
                    person1: person1,
                    person2: person2,
                    resSource: childResPile
                );
                parameters.state.waitingPeople.Add(newPerson);

                reprodCenter.RemovePerson(person: person1);
                reprodCenter.RemovePerson(person: person2);
            }

            while (reprodCenter.unpairedPeople.Count >= 2 && ResPile.Create(source: parameters.state.storedResPile, resAmounts: Person.resAmountsPerPerson) is ResPile childResPile)
            {
                Person person1 = reprodCenter.unpairedPeople.Dequeue(),
                    person2 = reprodCenter.unpairedPeople.Dequeue();
                birthQueue.Enqueue((person1, person2, childResPile: childResPile));
            }

            return this;
        }

        protected override void Delete()
        {
            base.Delete();
            reprodCenter.Delete();
            // TODO: need to disalow straight-up deletion.
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
                text += $"{birthQueue.Count} children are being born\n(maximum supported is {parameters.MaxCouples})\n";
                text += reprodCenter.GetInfo();
            }
            return text;
        }
    }
}
