namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : Production
    {
        [Serializable]
        public new sealed class Factory : Production.Factory
        {
            public readonly ResAmounts demandPerUnitSurface, supplyPerUnitSurface;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, ResAmounts demandPerUnitSurface, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Manufacturing,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    prodDuration: prodDuration
                )
            {
                this.demandPerUnitSurface = demandPerUnitSurface;
                this.supplyPerUnitSurface = supplyPerUnitSurface;
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
                => state.ApproxSurfaceLength * factory.demandPerUnitSurface;

            protected override ResAmounts SupplyPerUnitSurface
                => factory.supplyPerUnitSurface;

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
