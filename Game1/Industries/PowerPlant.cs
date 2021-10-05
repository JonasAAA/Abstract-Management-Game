using Game1.UI;
using Microsoft.Xna.Framework;
using System;

namespace Game1.Industries
{
    public class PowerPlant : Industry, IElectrProducer
    {
        public new class Params : Industry.Params
        {
            public readonly double prodWattsPerSec;

            public Params(string name, double reqSkill, double prodWattsPerSec)
                : base
                (
                      industryType: IndustryType.PowerPlant,
                      electrPriority: 0,
                      name: name,
                      reqSkill: reqSkill,
                      explanation: $"requires {reqSkill} skill\nproduces {prodWattsPerSec} W/s"
                )
            {
                if (prodWattsPerSec <= 0)
                    throw new ArgumentOutOfRangeException();
                this.prodWattsPerSec = prodWattsPerSec;
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
                UIPanel: new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
            )
        {
            this.parameters = parameters;
            ElectricityDistributor.AddElectrProducer(electrProducer: this);
        }

        public override ULongArray TargetStoredResAmounts()
            => new();

        protected override bool IsBusy()
            => true;

        protected override Industry Update(TimeSpan elapsed, double workingPropor)
        {
            if (!C.IsTiny(value: workingPropor - CurSkillPropor))
                throw new Exception();
            return this;
        }

        public override string GetText()
            => base.GetText() + parameters.name + "\n";

        public override double ReqWattsPerSec()
            => 0;

        double IElectrProducer.ProdWattsPerSec()
            => IsBusy() switch
            {
                true => parameters.prodWattsPerSec * CurSkillPropor,
                false => 0
            };
    }
}