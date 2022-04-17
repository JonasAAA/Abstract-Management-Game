using Game1.ChangingValues;

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

            protected override Params CreateParams(NodeState state)
                => new
                (
                    baseParams: base.CreateParams(state),
                    prodWatts: state.approxSurfaceLength * prodWattsPerUnitSurface
                );

            public override PowerPlant CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));
        }

        [Serializable]
        public new sealed record Params : ProductiveIndustry.Params
        {
            public readonly IReadOnlyChangingUDouble prodWatts;

            public Params(ProductiveIndustry.Params baseParams, IReadOnlyChangingUDouble prodWatts)
                : base(baseParams)
            {
                this.prodWatts = prodWatts;
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
                true => parameters.prodWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}