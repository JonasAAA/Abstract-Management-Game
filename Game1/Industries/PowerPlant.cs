using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [DataContract]
    public class PowerPlant : Industry, IEnergyProducer
    {
        [DataContract]
        public new class Params : Industry.Params
        {
            [DataMember]
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

            protected override Industry MakeIndustry(NodeState state)
                => new PowerPlant(state: state, parameters: this);
        }

        [DataMember]
        private readonly Params parameters;

        private PowerPlant(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
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