using Game1.Inhabitants;
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

            public override Params CreateParams(IIndustryFacingNodeState state)
                => new(state: state, factory: this);

            public ResAmounts BuildingCost(IIndustryFacingNodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, Building building)
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
                => base.TooltipText + $"{nameof(BuildingCost)}: {BuildingCost}\n{nameof(reqWattsPerChild)}: {reqWattsPerChild}\n{nameof(MaxCouples)}: {MaxCouples}\nresPerChild: {RealPerson.resAmountsPerPerson}\n{nameof(birthDuration)}: {birthDuration}";

            private readonly Factory factory;

            public Params(IIndustryFacingNodeState state, Factory factory)
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
            public RealPeople RealPeopleHere
                => realPeopleHere;
            public readonly UniqueQueue<VirtualPerson> unpairedPeople;

            private readonly Params parameters;

            public ReprodCenter(Params parameters)
                : base(activityType: ActivityType.Reproduction, energyPriority: parameters.energyPriority, state: parameters.state)
            {
                this.parameters = parameters;
                unpairedPeople = new();
            }

            public override bool IsFull()
                => allPeople.Count >= 2 * parameters.MaxCouples;

            public override bool IsPersonSuitable(VirtualPerson person)
                // could disalow far travel
                => true;

            public override Score PersonScoreOfThis(VirtualPerson person)
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

            public override void TakePersonFrom(RealPeople realPersonSource, RealPerson realPerson)
            {
                base.TakePersonFrom(realPersonSource: realPersonSource, realPerson: realPerson);
                unpairedPeople.Enqueue(element: realPerson.asVirtual);
            }

            protected override void UpdatePerson(RealPerson realPerson)
                => IActivityCenter.UpdatePersonDefault(realPerson: realPerson);

            public override bool CanPersonLeave(VirtualPerson person)
                // a person can't leave while in the process of having a child
                => !realPeopleHere.Contains(person: person) || unpairedPeople.Contains(element: person);

            protected override void RemovePersonInternal(VirtualPerson person, bool force)
                => unpairedPeople.TryRemove(element: person);

            public string GetInfo()
                => $"{unpairedPeople.Count} waiting people\n{allPeople.Count - realPeopleHere.Count} people travelling here\n";
        }

        public override bool PeopleWorkOnTop
            => false;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;
        private readonly ReprodCenter reprodCenter;
        private readonly TimedQueue<(VirtualPerson, VirtualPerson, ReservedResPile childResPile)> birthQueue;

        private ReprodIndustry(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            reprodCenter = new(parameters: parameters);

            birthQueue = new(duration: parameters.birthDuration);
        }

        public override void UpdatePeople(RealPerson.UpdateParams updateParams)
        {
            base.UpdatePeople(updateParams: updateParams);

            reprodCenter.UpdatePeople(updateParams: updateParams);
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.MaxCouples * RealPerson.resAmountsPerPerson * parameters.state.MaxBatchDemResStored;

        protected override BoolWithExplanationIfFalse IsBusy()
            => base.IsBusy() & BoolWithExplanationIfFalse.Create
            (
                value: birthQueue.Count > 0,
                explanationIfFalse: "need 2 people to have a child"
            );

        protected override ReprodIndustry InternalUpdate(Propor workingPropor)
        {
            birthQueue.Update(workingPropor: workingPropor);

            foreach (var (parent1, parent2, childResPile) in birthQueue.DoneElements())
            {
                RealPerson.GenerateChild
                (
                    nodeID: parameters.state.NodeID,
                    parent1: parent1,
                    parent2: parent2,
                    resSource: childResPile,
                    parentSource: reprodCenter.RealPeopleHere,
                    childDestin: parameters.state.WaitingPeople
                );

                reprodCenter.RemovePerson(person: parent1, force: true);
                reprodCenter.RemovePerson(person: parent2, force: true);
            }

            while (reprodCenter.unpairedPeople.Count >= 2 && ReservedResPile.CreateIfHaveEnough(source: parameters.state.StoredResPile, resAmounts: RealPerson.resAmountsPerPerson) is ReservedResPile childResPile)
            {
                // TODO: move this logic into ReprodCenter class?
                VirtualPerson person1 = reprodCenter.unpairedPeople.Dequeue(),
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
        
        protected override UDouble ReqWatts()
            => (UDouble)birthQueue.Count * parameters.reqWattsPerChild * CurSkillPropor;

        protected override string GetBusyInfo()
            => CurWorldManager.Overlay switch
            {
                IPeopleOverlay => $"{birthQueue.Count} children are being born\n(maximum supported is {parameters.MaxCouples})\n" + reprodCenter.GetInfo(),
                _ => ""
            };
    }
}
