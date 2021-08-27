﻿namespace Game1
{
    public class PowerPlant : Industry
    {
        public new class Params : Industry.Params
        {
            public Params(string name, double reqSkill, double prodWattsPerSec)
                : base(industryType: IndustryType.PowerPlant, name: name, reqSkill: reqSkill, reqWattsPerSec: 0, prodWattsPerSec: prodWattsPerSec)
            { }

            public override Industry MakeIndustry(NodeState state)
                => new PowerPlant(parameters: this, state: state);
        }

        private readonly Params parameters;

        private PowerPlant(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
        }

        public override ULongArray TargetStoredResAmounts()
            => new();

        protected override bool IsBusy()
            => true;

        public override string GetText()
            => base.GetText() + parameters.name;
    }
}