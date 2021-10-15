using Game1.UI;
using Microsoft.Xna.Framework;
using System;

using static Game1.WorldManager;

namespace Game1.Industries
{
    public class PowerPlant : Industry, IEnergyProducer
    {
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

            public override Industry MakeIndustry(NodeState state)
                => new PowerPlant(parameters: this, state: state);
        }

        private readonly Params parameters;

        private PowerPlant(Params parameters, NodeState state)
            : base
            (
                parameters: parameters,
                state: state,
                UIPanel: new UIRectVertPanel<IHUDElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
            )
        {
            this.parameters = parameters;
            AddEnergyProducer(energyProducer: this);
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

        public override string GetText()
            => base.GetText() + parameters.name + "\n";

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