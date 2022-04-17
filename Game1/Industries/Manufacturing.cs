using Game1.ChangingValues;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : Production
    {
        [Serializable]
        public new sealed class Factory : Production.Factory
        {
            public readonly ResAmounts demandPerUnitSurface;

            private readonly ResAmounts supplyPerUnitSurface;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, ResAmounts demandPerUnitSurface, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    prodDuration: prodDuration,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(supplyPerUnitSurface)} {supplyPerUnitSurface}\n{nameof(demandPerUnitSurface)} {demandPerUnitSurface}\n{nameof(prodDuration)} {prodDuration}"
                )
            {
                this.demandPerUnitSurface = demandPerUnitSurface;
                this.supplyPerUnitSurface = supplyPerUnitSurface;
            }

            protected override ResAmounts SupplyPerUnitSurface(NodeState state)
                => supplyPerUnitSurface;

            protected override Params CreateParams(NodeState state)
                => new
                (
                    baseParams: base.CreateParams(state),
                    demand: state.approxSurfaceLength * demandPerUnitSurface
                );

            public override Manufacturing CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));
        }

        [Serializable]
        public new sealed record Params : Production.Params
        {
            public readonly IReadOnlyChangingResAmounts demand;

            public Params(Production.Params baseParams, IReadOnlyChangingResAmounts demand)
                : base(baseParams)
            {
                this.demand = demand;
            }
        }

        private readonly Params parameters;

        private Manufacturing(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.demand.Value * parameters.state.maxBatchDemResStored;

        protected override bool CanStartProduction()
            => parameters.state.storedRes >= parameters.demand.Value;

        protected override void StartProduction()
            => parameters.state.storedRes -= parameters.demand.Value;

        protected override void StopProduction()
        { }
    }
}
