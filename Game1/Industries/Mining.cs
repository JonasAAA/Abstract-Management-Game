namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : Production
    {
        [Serializable]
        public new sealed class Params : Production.Params, IBuildableParams
        {
            public readonly BasicResInd minedRes;

            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, BasicResInd minedRes, ulong minedResPerUnitSurface, TimeSpan miningDuration)
                : base
                (
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    supplyPerUnitSurface: new()
                    {
                        [minedRes] = minedResPerUnitSurface
                    },
                    prodDuration: miningDuration,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(minedRes)} {minedRes}\n{nameof(minedResPerUnitSurface)} {minedResPerUnitSurface}\n{nameof(miningDuration)} {miningDuration}"
                )
            {
                this.minedRes = minedRes;
            }

            public override bool CanCreateWith(NodeState state)
                => state.consistsOf == minedRes;

            protected override Mining InternalCreateIndustry(NodeState state)
                => new(state: state, parameters: this);

            string IBuildableParams.ButtonName
                => name;
        }

        private readonly Params parameters;

        private Mining(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
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
