using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : Production
    {
        [Serializable]
        public new sealed class Factory : Production.Factory
        {
            public readonly NonBasicResInd producedResInd;
            public readonly ulong prodResPerUnitSurface;

            public Factory(string name, NonBasicResInd producedResInd, ulong prodResPerUnitSurface, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Manufacturing,
                    name: name,
                    color: Color.Brown,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    prodDuration: prodDuration
                )
            {
                this.producedResInd = producedResInd;
                this.prodResPerUnitSurface = prodResPerUnitSurface;
            }

            public override Manufacturing CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);
        }

        [Serializable]
        public new sealed class Params : Production.Params
        {
            public ResAmounts Demand
                => state.ApproxSurfaceLength * factory.prodResPerUnitSurface * CurResConfig.resources[factory.producedResInd].recipe;

            protected override ResAmounts SupplyPerUnitSurface
                => factory.prodResPerUnitSurface * new ResAmounts()
                {
                    [factory.producedResInd] = factory.prodResPerUnitSurface
                };

            public override string TooltipText
                => base.TooltipText + $"{nameof(Demand)}: {Demand}\n";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        private readonly Params parameters;

        private Manufacturing(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.Demand * parameters.state.maxBatchDemResStored;

        protected override bool CanStartProduction()
            => parameters.state.storedRes >= parameters.Demand;

        protected override void StartProduction()
            => parameters.state.storedRes -= parameters.Demand;

        protected override void StopProduction()
        { }
    }
}
