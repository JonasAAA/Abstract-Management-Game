using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class House : Industry
    {
        [Serializable]
        public new sealed class Factory : Industry.Factory, IFactoryForIndustryWithBuilding
        {
            public readonly UDouble floorSpacePerUnitSurface;
            private readonly ResAmounts buildingCostPerUnitSurface;

            public Factory(string name, UDouble floorSpacePerUnitSurface, ResAmounts buildingCostPerUnitSurface)
                : base(name: name, color: CurWorldConfig.houseIndustryColor)
            {
                this.floorSpacePerUnitSurface = floorSpacePerUnitSurface;
                if (buildingCostPerUnitSurface.IsEmpty())
                    throw new ArgumentException();
                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
            }

            public override Params CreateParams(IIndustryFacingNodeState state)
                => new(state: state, factory: this);

            public ResAmounts BuildingCost(IIndustryFacingNodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, Building building)
            {
                if (building.Cost != BuildingCost(state: state))
                    throw new ArgumentException();
                return new House(parameters: CreateParams(state: state), building: building);
            }
        }

        [Serializable]
        public new sealed class Params : Industry.Params
        {
            public UDouble FloorSpace
                => state.ApproxSurfaceLength * factory.floorSpacePerUnitSurface;
            public override string TooltipText
                => base.TooltipText + $"BuildingCost: {factory.BuildingCost(state: state)}\n{nameof(FloorSpace)}: {FloorSpace}\n";

            private readonly Factory factory;

            public Params(IIndustryFacingNodeState state, Factory factory)
                : base(state: state, factory: factory)
                => this.factory = factory;
        }

        [Serializable]
        private sealed class Housing : ActivityCenter
        {
            private readonly Params parameters;

            public Housing(Params parameters)
                : base(activityType: ActivityType.Unemployed, energyPriority: EnergyPriority.maximal, state: parameters.state)
                => this.parameters = parameters;

            public override bool IsFull()
                => false;

            public Score PersonalSpace()
                => PersonalSpace(peopleCount: realPeopleHere.Count);

            private Score PersonalSpace(ulong peopleCount)
                // TODO: get rid of hard-coded constant
                => Score.FromUnboundedUDouble(value: parameters.FloorSpace / peopleCount, valueGettingAverageScore: 10);

            public override Score PersonEnjoymentOfThis(VirtualPerson person)
                // TODO: get rid of hard-coded constants
                => Score.WeightedAverage
                (
                    (weight: 5, score: Score.lowest),
                    (weight: 3, score: PersonalSpace(peopleCount: allPeople.Count + 1))
                );

            public override bool IsPersonSuitable(VirtualPerson person)
                // may disallow far travel
                => true;

            protected override UpdatePersonSkillsParams? PersonUpdateParams(RealPerson realPerson)
                => null;

            public override bool CanPersonLeave(VirtualPerson person)
                => true;

            public string GetInfo()
                => $"{realPeopleHere.Count} people live here\n{allPeople.Count - realPeopleHere.Count} people travel here\n";
        }

        public override bool PeopleWorkOnTop
            => true;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Housing housing;
        
        private House(Params parameters, Building building)
            : base(parameters: parameters, building: building)
            => housing = new(parameters: parameters);

        public override void UpdatePeople(RealPerson.UpdateLocationParams updateLocationParams)
            => housing.UpdatePeople(updateLocationParams: updateLocationParams);

        public override ResAmounts TargetStoredResAmounts()
            => ResAmounts.Empty;

        protected override House InternalUpdate()
            => this;

        protected override void Delete()
        {
            base.Delete();
            housing.Delete();
        }

        public override string GetInfo()
            => housing.GetInfo() + $"each person gets {housing.PersonalSpace():#.##} floor space\n";
    }
}
