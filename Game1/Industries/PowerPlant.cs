using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class PowerPlant : ProductiveIndustry, IEnergyProducer
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
        {
            public readonly Propor surfaceAbsorbtionPropor;
            private readonly ResAmounts buildingCostPerUnitSurface;

            public Factory(string name, UDouble reqSkillPerUnitSurface, Propor surfaceAbsorbtionPropor, ResAmounts buildingCostPerUnitSurface)
                : base
                (
                    industryType: IndustryType.PowerPlant,
                    energyPriority: EnergyPriority.mostImportant,
                    name: name,
                    color: Color.Blue,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                if (surfaceAbsorbtionPropor.IsCloseTo(other: Propor.empty))
                    throw new ArgumentOutOfRangeException();
                this.surfaceAbsorbtionPropor = surfaceAbsorbtionPropor;
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
            public readonly Propor surfaceAbsorbtionPropor;

            // TODO: may improve the tooltip text by showing the actual produced amount
            public override string TooltipText
                => $"""
                {base.TooltipText}
                {nameof(surfaceAbsorbtionPropor)}: {surfaceAbsorbtionPropor}
                """;

            public Params(IIndustryFacingNodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                surfaceAbsorbtionPropor = factory.surfaceAbsorbtionPropor;
            }
        }

        public override bool PeopleWorkOnTop
            => false;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;
        private ElectricalEnergy prodEnergy;

        private PowerPlant(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            prodEnergy = ElectricalEnergy.zero;
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
            => $"produce {prodEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W\n";

        protected override UDouble ReqWatts()
            => 0;

        void IEnergyProducer.ProduceEnergy<T>(T destin)
        {
            prodEnergy = parameters.state.LocationCounters.TransformRadiantToElectricalEnergyAndTransfer
            (
                destin: destin,
                proporToTransform: parameters.surfaceAbsorbtionPropor * CurSkillPropor
            );
        }
    }
}