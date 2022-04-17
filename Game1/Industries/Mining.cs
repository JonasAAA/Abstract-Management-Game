using Game1.ChangingValues;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : Production
    {
        [Serializable]
        public new sealed class Factory : Production.Factory, IBuildableFactory
        {
            private readonly ulong minedResPerUnitSurface;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ulong minedResPerUnitSurface, TimeSpan miningDuration)
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

            protected override ResAmounts SupplyPerUnitSurface(NodeState state)
                => new()
                {
                    [state.consistsOf] = minedResPerUnitSurface
                };

            protected override Params CreateParams(NodeState state)
                => new
                (
                    baseParams: base.CreateParams(state: state),
                    minedRes: state.approxSurfaceLength * minedResPerUnitSurface
                );

            public override Mining CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            string IBuildableFactory.ButtonName
                => name;
        }

        [Serializable]
        public new sealed record Params : Production.Params
        {
            public readonly IReadOnlyChangingULong minedRes;

            public Params(Production.Params baseParams, IReadOnlyChangingULong minedRes)
                : base(baseParams)
            {
                this.minedRes = minedRes;
            }
        }

        private readonly Params parameters;

        private Mining(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        // TODO: could mine less if not enough resources remaining for 
        protected override bool CanStartProduction()
            => parameters.state.CanRemove(resAmount: parameters.minedRes.Value);

        protected override void StartProduction()
        { }

        protected override void StopProduction()
        {
            parameters.state.Remove(resAmount: parameters.minedRes.Value);
            parameters.state.storedRes += parameters.supply.Value;
        }
    }
}
