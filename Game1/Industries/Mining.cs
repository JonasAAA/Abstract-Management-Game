using Game1.ChangingValues;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : Production
    {
        [Serializable]
        public new sealed class Params : Production.Params, IBuildableParams
        {
            private readonly ulong minedResPerUnitSurface;

            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ulong minedResPerUnitSurface, TimeSpan miningDuration)
                : base
                (
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    prodDuration: miningDuration,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(minedResPerUnitSurface)} {minedResPerUnitSurface}\n{nameof(miningDuration)} {miningDuration}"
                )
            {
                this.minedResPerUnitSurface = minedResPerUnitSurface;
            }

            // TODO: get rid of this if always return true
            public override bool CanCreateWith(NodeState state)
                => true;

            protected override Mining InternalCreateIndustry(NodeState state)
                => new
                (
                    state: state,
                    parameters: this,
                    reqWatts: state.approxSurfaceLength * reqSkillPerUnitSurface,
                    supply: state.approxSurfaceLength * new ResAmounts()
                    {
                        [state.consistsOf] = minedResPerUnitSurface
                    }
                );

            string IBuildableParams.ButtonName
                => name;
        }

        private readonly Params parameters;

        private Mining(NodeState state, Params parameters, IReadOnlyChangingUDouble reqWatts, IReadOnlyChangingResAmounts supply)
            : base(state: state, parameters: parameters, reqWatts: reqWatts, supply: supply)
        {
            this.parameters = parameters;
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override bool CanStartProduction()
            => throw new NotImplementedException();

        protected override void StartProduction()
            => throw new NotImplementedException();

        protected override void StopProduction()
            => throw new NotImplementedException();
    }
}
