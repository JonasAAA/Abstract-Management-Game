using Game1.ChangingValues;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Factory : Production
    {
        [Serializable]
        public new sealed class Params : Production.Params
        {
            public readonly ResAmounts demandPerUnitSurface;

            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, ResAmounts demandPerUnitSurface, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    supplyPerUnitSurface: supplyPerUnitSurface,
                    prodDuration: prodDuration,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(supplyPerUnitSurface)} {supplyPerUnitSurface}\n{nameof(demandPerUnitSurface)} {demandPerUnitSurface}\n{nameof(prodDuration)} {prodDuration}"
                )
            {
                this.demandPerUnitSurface = demandPerUnitSurface;
            }

            public override bool CanCreateWith(NodeState state)
                => true;

            protected override Factory InternalCreateIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingResAmounts demand;

        private Factory(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            demand = parameters.demandPerUnitSurface * state.approxSurfaceLength;
        }

        public override ResAmounts TargetStoredResAmounts()
            => demand.Value * state.maxBatchDemResStored;

        protected override bool CanStartProduction()
            => state.storedRes >= demand.Value;

        protected override void StartProduction()
            => state.storedRes -= demand.Value;

        protected override void StopProduction()
        { }
    }
}
