using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class PowerPlant : ProductiveIndustry, IEnergyProducer
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory
        {
            public readonly UDouble prodWattsPerUnitSurface;
            
            public Factory(string name, UDouble reqSkillPerUnitSurface, UDouble prodWattsPerUnitSurface)
                : base
                (
                    industryType: IndustryType.PowerPlant,
                    energyPriority: EnergyPriority.minimal,
                    name: name,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(prodWattsPerUnitSurface)} {prodWattsPerUnitSurface}"
                )
            {
                if (prodWattsPerUnitSurface.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();
                this.prodWattsPerUnitSurface = prodWattsPerUnitSurface;
            }

            public override PowerPlant CreateIndustry(NodeState state)
                => new(parameters: new(state: state, factory: this));
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ProdWatts
                => state.ApproxSurfaceLength * factory.prodWattsPerUnitSurface;

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

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
            => base.GetInfo() + parameters.name + "\n";

        public override UDouble ReqWatts()
            => 0;

        UDouble IEnergyProducer.ProdWatts()
            => IsBusy() switch
            {
                true => parameters.ProdWatts * CurSkillPropor,
                false => 0
            };
    }
}