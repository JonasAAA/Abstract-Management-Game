using System;
using System.Collections.Generic;

namespace Game1
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly ConstUIntArray supply, demand;
            public readonly TimeSpan prodTime;

            public Params(string name, List<Upgrade> upgrades, ConstUIntArray supply, ConstUIntArray demand, TimeSpan prodTime)
                : base(name, upgrades)
            {
                this.supply = supply;
                this.demand = demand;
                if (prodTime.TotalSeconds < 0)
                    throw new ArgumentException();
                this.prodTime = prodTime;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Factory(parameters: this, state: state);
        }

        protected override bool IsProducing
            => !production.Empty;

        private readonly Params parameters;
        private readonly TimedResQueue production;

        public Factory(Params parameters, NodeState state)
            : base(parameters, state)
        {
            this.parameters = parameters;
            production = new(duration: parameters.prodTime);
        }

        protected override void StartProduction()
        {
            base.StartProduction();

            if (production.Empty && state.stored >= parameters.demand)
            {
                state.stored -= parameters.demand;
                production.Enqueue(newResAmounts: parameters.supply);
            }
        }

        public override Industry FinishProduction()
        {
            state.arrived += production.DoneResAmounts();

            return base.FinishProduction();
        }

        public override string GetText()
        {
            string text = $"{parameters.name}\n";
            if (production.Empty)
                text += "idle";
            else
                text += $"producing {production.PeekCompletionProp() * 100: 0.}%";
            return text;
        }
    }
}
