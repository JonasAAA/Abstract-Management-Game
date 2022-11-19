using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class PowerPlant : ProductiveIndustry, IEnergyProducer
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
        {
            public readonly Propor surfaceWattsAbsorbedPropor;
            private readonly ResAmounts buildingCostPerUnitSurface;

            public Factory(string name, UDouble reqSkillPerUnitSurface, Propor surfaceWattsAbsorbedPropor, ResAmounts buildingCostPerUnitSurface)
                : base
                (
                    industryType: IndustryType.PowerPlant,
                    energyPriority: EnergyPriority.minimal,
                    name: name,
                    color: Color.Blue,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                if (surfaceWattsAbsorbedPropor.IsCloseTo(other: Propor.empty))
                    throw new ArgumentOutOfRangeException();
                this.surfaceWattsAbsorbedPropor = surfaceWattsAbsorbedPropor;
                if (buildingCostPerUnitSurface.IsEmpty())
                    throw new ArgumentException();
                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
            }

            public override Params CreateParams(IIndustryFacingNodeState state)
                => new(state: state, factory: this);

            ResAmounts IFactoryForIndustryWithBuilding.BuildingCost(IIndustryFacingNodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, Building building)
                => new PowerPlant(parameters: CreateParams(state: state), building: building);
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ProdWatts
                => state.WattsHittingSurfaceOrIndustry * factory.surfaceWattsAbsorbedPropor;

            private readonly Factory factory;

            public override string TooltipText
                => $"""
                {base.TooltipText}
                {nameof(ProdWatts)}: {ProdWatts}
                """;

            public Params(IIndustryFacingNodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        public override bool PeopleWorkOnTop
            => false;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;

        private PowerPlant(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            CurWorldManager.AddEnergyProducer(energyProducer: this);
        }

        public override ResAmounts TargetStoredResAmounts()
            => ResAmounts.Empty;

        protected override PowerPlant InternalUpdate(Propor workingPropor)
        {
            if (!MyMathHelper.AreClose(workingPropor, CurSkillPropor))
                throw new Exception();
            return this;
        }

        protected override string GetBusyInfo()
            => $"produce {ProdWatts:0.##} W\n";

        protected override UDouble ReqWatts()
            => 0;

        UDouble IEnergyProducer.ProdWatts()
            => ProdWatts;

        private UDouble ProdWatts
            => IsBusy().SwitchExpression
            (
                trueCase: () => parameters.ProdWatts * CurSkillPropor,
                falseCase: () => (UDouble)0
            );
    }
}