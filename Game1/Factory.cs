using System;

namespace Game1
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly ConstULongArray supply, demand;
            public readonly TimeSpan prodDuration;

            public Params(string name, ulong electrPriority, double reqSkill, ulong reqWattsPerSec, ConstULongArray supply, ConstULongArray demand, TimeSpan prodDuration)
                : base(industryType: IndustryType.Production, name: name, electrPriority: electrPriority, reqSkill: reqSkill, reqWattsPerSec: reqWattsPerSec)
            {
                this.supply = supply;
                this.demand = demand;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Factory(parameters: this, state: state);
        }

        private readonly Params parameters;
        private TimeSpan prodTimeLeft;

        private Factory(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            prodTimeLeft = TimeSpan.MaxValue;
        }

        public override ULongArray TargetStoredResAmounts()
        {
            if (CanStartProduction)
                return parameters.demand * state.maxBatchDemResStored;
            return new();
        }

        protected override bool IsBusy()
            => prodTimeLeft < TimeSpan.MaxValue;

        protected override Industry Update(TimeSpan elapsed, double workingPropor)
        {
            if (IsBusy())
                prodTimeLeft -= workingPropor * elapsed;

            if (CanStartProduction && !IsBusy() && state.storedRes >= parameters.demand)
            {
                state.storedRes -= parameters.demand;
                prodTimeLeft = parameters.prodDuration;
            }

            if (prodTimeLeft <= TimeSpan.Zero)
            {
                state.waitingResAmountsPackets.Add(destination: state.position, resAmounts: parameters.supply);
                prodTimeLeft = TimeSpan.MaxValue;
            }

            return this;
        }

        public override string GetText()
        {
            string text = base.GetText() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"producing {C.DonePart(timeLeft: prodTimeLeft, duration: parameters.prodDuration) * 100: 0.}%\n";
            else
                text += "idle\n";
            if (!CanStartProduction)
                text += "will not start new\n";
            return text;
        }
    }
}