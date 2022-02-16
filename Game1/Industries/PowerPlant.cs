using System;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class PowerPlant : Industry, IEnergyProducer
    {
        [Serializable]
        public new class Params : Industry.Params
        {
            public readonly double prodWatts;

            public Params(string name, double reqSkill, double prodWatts)
                : base
                (
                      industryType: IndustryType.PowerPlant,
                      energyPriority: 0,
                      name: name,
                      reqSkill: reqSkill,
                      explanation: $"requires {reqSkill} skill\nproduces {prodWatts} W/s"
                )
            {
                if (prodWatts <= 0)
                    throw new ArgumentOutOfRangeException();
                this.prodWatts = prodWatts;
            }

            public override PowerPlant MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;

        private PowerPlant(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            CurWorldManager.AddEnergyProducer(energyProducer: this);
        }

        public override ULongArray TargetStoredResAmounts()
            => new();

        protected override bool IsBusy()
            => true;

        protected override Industry Update(double workingPropor)
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
                true => parameters.prodWatts * CurSkillPropor,
                false => 0
            };
    }
}