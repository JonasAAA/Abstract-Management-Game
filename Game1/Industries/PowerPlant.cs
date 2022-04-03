using Game1.ChangingValues;
using Game1.PrimitiveTypeWrappers;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class PowerPlant : ProductiveIndustry, IEnergyProducer
    {
        [Serializable]
        public new class Params : ProductiveIndustry.Params
        {
            public readonly UFloat prodWattsPerUnitSurface;
            
            public Params(string name, UFloat reqSkillPerUnitSurface, UFloat prodWattsPerUnitSurface)
                : base
                (
                    industryType: IndustryType.PowerPlant,
                    energyPriority: EnergyPriority.minimal,
                    name: name,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(prodWattsPerUnitSurface)} {prodWattsPerUnitSurface}"
                )
            {
                if (prodWattsPerUnitSurface <= 0)
                    throw new ArgumentOutOfRangeException();
                this.prodWattsPerUnitSurface = prodWattsPerUnitSurface;
            }

            public override PowerPlant MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingUFloat prodWatts;

        private PowerPlant(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            prodWatts = parameters.prodWattsPerUnitSurface * state.approxSurfaceLength;
            CurWorldManager.AddEnergyProducer(energyProducer: this);
        }

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override bool IsBusy()
            => true;

        protected override PowerPlant InternalUpdate(double workingPropor)
        {
            if (!C.IsTiny(value: workingPropor - CurSkillPropor))
                throw new Exception();
            return this;
        }

        public override string GetInfo()
            => base.GetInfo() + parameters.name + "\n";

        public override double ReqWatts()
            => 0;

        double IEnergyProducer.ProdWatts()
            => IsBusy() switch
            {
                true => prodWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}