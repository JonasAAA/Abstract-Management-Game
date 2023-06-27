//using Game1.Inhabitants;
//using static Game1.WorldManager;
//using static Game1.UI.ActiveUIManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class House : Industry
//    {
//        [Serializable]
//        public new sealed class Factory : Industry.Factory, IFactoryForIndustryWithBuilding
//        {
//            public readonly UDouble floorSpacePerUnitSurface;
//            private readonly SomeResAmounts<IResource> buildingCostPerUnitSurface;

//            public Factory(string Name, UDouble floorSpacePerUnitSurface, SomeResAmounts<IResource> buildingCostPerUnitSurface)
//                : base(Name: Name, Color: colorConfig.houseIndustryColor)
//            {
//                this.floorSpacePerUnitSurface = floorSpacePerUnitSurface;
//                if (buildingCostPerUnitSurface.IsEmpty())
//                    throw new ArgumentException();
//                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            public SomeResAmounts<IResource> BuildingCost(IIndustryFacingNodeState state)
//                => state.SurfaceLength * buildingCostPerUnitSurface;

//            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, BuildingShape building)
//            {
//                if (building.Cost != BuildingCost(state: state))
//                    throw new ArgumentException();
//                return new House(parameters: CreateParams(state: state), building: building);
//            }
//        }

//        [Serializable]
//        public new sealed class GeneralParams : Industry.GeneralParams
//        {
//            public UDouble FloorSpace
//                => state.SurfaceLength * factory.floorSpacePerUnitSurface;
//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                {nameof(factory.BuildingCost)}: {factory.BuildingCost(state: state)}
//                {nameof(FloorSpace)}: {FloorSpace}
//                """;

//            private readonly Factory factory;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//                => this.factory = factory;
//        }

//        [Serializable]
//        private sealed class Housing : ActivityCenter
//        {
//            private readonly GeneralParams parameters;

//            public Housing(GeneralParams parameters, IEnergyDistributor energyDistributor)
//                : base(energyDistributor: energyDistributor, activityType: ActivityType.Unemployed, energyPriority: EnergyPriority.leastImportant, state: parameters.state)
//                => this.parameters = parameters;

//            public override bool IsFull()
//                => false;

//            public Score PersonalSpace()
//                => PersonalSpace(numPeople: PeopleHereStats.totalNumPeople.value);

//            private Score PersonalSpace(ulong numPeople)
//                // TODO: get rid of hard-coded constant
//                => Score.FromUnboundedUDouble(value: parameters.FloorSpace / numPeople, valueGettingAverageScore: 10);

//            public override Score PersonEnjoymentOfThis(VirtualPerson person)
//                // TODO: get rid of hard-coded constants
//                => Score.WeightedAverage
//                (
//                    (weight: 5, score: Score.lowest),
//                    (weight: 3, score: PersonalSpace(numPeople: allPeople.Count.value + 1))
//                );

//            public override bool IsPersonSuitable(VirtualPerson person)
//                // may disallow far travel
//                => true;

//            protected override UpdatePersonSkillsParams? UpdatePersonSkillsParams
//                => null;

//            public override bool CanPersonLeave(VirtualPerson person)
//                => true;

//            public string GetInfo()
//                => $"""
//                {PeopleHereStats}
//                people travelling to here {allPeople.Count - PeopleHereStats.totalNumPeople}

//                """;
//        }

//        public override bool PeopleWorkOnTop
//            => true;

//        public override RealPeopleStats Stats
//            => housing.PeopleHereStats;

//        protected override UDouble Height
//            => CurWorldConfig.defaultIndustryHeight;

//        private readonly Housing housing;

//        private House(GeneralParams parameters, BuildingShape building)
//            : base(parameters: parameters, building: building)
//            => housing = new(parameters: parameters, energyDistributor: combinedEnergyConsumer);

//        protected override void UpdatePeopleInternal()
//            => housing.UpdatePeople();

//        public override ResAmounts TargetStoredResAmounts()
//            => ResAmounts.empty;

//        protected override House InternalUpdate()
//            => this;

//        protected override void Delete()
//        {
//            base.Delete();
//            housing.Delete();
//        }

//        public override string GetInfo()
//            => housing.GetInfo() + $"each person gets {housing.PersonalSpace():#.##} floor space\n";
//    }
//}
