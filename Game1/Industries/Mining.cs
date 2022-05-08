using Game1.UI;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : Production
    {
        [Serializable]
        public new sealed class Factory : Production.Factory, IBuildableFactory
        {
            public readonly ulong minedResPerUnitSurface;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ulong minedResPerUnitSurface, TimeSpan miningDuration)
                : base
                (
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    reqWattsPerUnitSurface: reqWattsPerUnitSurface,
                    prodDuration: miningDuration
                )
            {
                this.minedResPerUnitSurface = minedResPerUnitSurface;
            }

            public override Mining CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => name;

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);
        }

        [Serializable]
        public new sealed class Params : Production.Params
        {
            public ulong MinedRes
                => state.ApproxSurfaceLength * factory.minedResPerUnitSurface;

            // TODO(optimization) could have supplyPerUnitSurface redonly field and return it here
            protected override ResAmounts SupplyPerUnitSurface
                => new()
                {
                    [state.consistsOfResInd] = factory.minedResPerUnitSurface
                };

            public override string TooltipText
                => base.TooltipText + $"{nameof(MinedRes)}: {MinedRes}";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
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
            => parameters.state.CanRemove(resAmount: parameters.MinedRes);

        protected override void StartProduction()
        { }

        protected override void StopProduction()
        {
            parameters.state.Remove(resAmount: parameters.MinedRes);
            parameters.state.storedRes += parameters.Supply;
        }
    }
}
