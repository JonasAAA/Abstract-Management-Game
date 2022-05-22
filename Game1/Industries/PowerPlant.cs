using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class PowerPlant : ProductiveIndustry, IEnergyProducer
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory
        {
            public readonly Propor surfaceWattsAbsorbedPropor;

            public Factory(string name, UDouble reqSkillPerUnitSurface, Propor surfaceWattsAbsorbedPropor)
                : base
                (
                    industryType: IndustryType.PowerPlant,
                    energyPriority: EnergyPriority.minimal,
                    name: name,
                    color: Color.Blue,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                if (surfaceWattsAbsorbedPropor.IsCloseTo(other: Propor.empty))
                    throw new ArgumentOutOfRangeException();
                this.surfaceWattsAbsorbedPropor = surfaceWattsAbsorbedPropor;
            }

            public override PowerPlant CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ProdWatts
                => state.wattsHittingSurfaceOrIndustry * factory.surfaceWattsAbsorbedPropor;

            private readonly Factory factory;

            public override string TooltipText
                => base.TooltipText + $"{nameof(ProdWatts)}: {ProdWatts}\n";

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        public override bool PeopleWorkOnTop
            => false;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;

        private PowerPlant(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            CurWorldManager.AddEnergyProducer(energyProducer: this);
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override bool IsBusy()
            => true;

        protected override PowerPlant InternalUpdate(Propor workingPropor)
        {
            if (!MyMathHelper.AreClose(workingPropor, CurSkillPropor))
                throw new Exception();
            return this;
        }

        public override string GetInfo()
            => base.GetInfo() + parameters.name + $"\nproduce {ProdWatts} W\n";

        public override UDouble ReqWatts()
            => 0;

        UDouble IEnergyProducer.ProdWatts()
            => ProdWatts;

        private UDouble ProdWatts
            => IsBusy() switch
            {
                true => parameters.ProdWatts * CurSkillPropor,
                false => 0
            };
    }
}