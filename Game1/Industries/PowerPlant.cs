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
            public readonly UDouble prodWattsPerUnitSurface;
            
            public Params(string name, UDouble reqSkillPerUnitSurface, UDouble prodWattsPerUnitSurface)
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
        private readonly IReadOnlyChangingUDouble prodWatts;

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
                true => prodWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}