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

            private readonly ResAmounts supplyPerUnitSurface;

            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, ResAmounts demandPerUnitSurface, TimeSpan prodDuration)
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

            public override bool CanCreateWith(NodeState state)
                => true;

            protected override Factory InternalCreateIndustry(NodeState state)
                => new
                (
                    state: state,
                    parameters: this,
                    reqWatts: state.approxSurfaceLength * reqWattsPerUnitSurface,
                    supply: state.approxSurfaceLength * supplyPerUnitSurface
                );
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingResAmounts demand;

        private Factory(NodeState state, Params parameters, IReadOnlyChangingUDouble reqWatts, IReadOnlyChangingResAmounts supply)
            : base(state: state, parameters: parameters, reqWatts: reqWatts, supply: supply)
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
