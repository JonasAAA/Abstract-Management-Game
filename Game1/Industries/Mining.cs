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
                    baseParams: base.CreateParams(state: state)
                );

            public override Mining CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            string IBuildableFactory.ButtonName
                => name;
        }

        [Serializable]
        public new sealed record Params : Production.Params
        {
            public Params(Production.Params baseParams)
                : base(baseParams)
            { }
        }

        private readonly Params parameters;

        private Mining(Params parameters)
            : base(parameters: parameters)
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
