using System;

namespace Game1
{
    public class Construction : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly Industry.Params industrParams;
            public readonly TimeSpan duration;
            public readonly ConstULongArray cost;

            public Params(string name, ulong electrPriority, double reqSkill, ulong reqWattsPerSec, Industry.Params industrParams, TimeSpan duration, ConstULongArray cost)
                : base(industryType: IndustryType.Construction, name: name, electrPriority: electrPriority, reqSkill: reqSkill, reqWattsPerSec: reqWattsPerSec)
            {
                this.industrParams = industrParams;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Construction(parameters: this, state: state);
        }

        private readonly Params parameters;
        private TimeSpan constrTimeLeft;

        private Construction(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ULongArray TargetStoredResAmounts()
            => parameters.cost.ToULongArray();

        protected override bool IsBusy()
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry Update(TimeSpan elapsed, double workingPropor)
        {
            if (IsBusy())
                constrTimeLeft -= workingPropor * elapsed;

            if (!IsBusy() && state.storedRes >= parameters.cost)
            {
                state.storedRes -= parameters.cost;
                constrTimeLeft = parameters.duration;
            }

            if (constrTimeLeft <= TimeSpan.Zero)
            {
                Clear();
                return parameters.industrParams.MakeIndustry(state: state);
            }
            return this;
        }

        public override string GetText()
        {
            string text = base.GetText();
            if (IsBusy())
                text += $"constructing {C.DonePart(timeLeft: constrTimeLeft, duration: parameters.duration) * 100: 0.}%\n";
            else
                text += "waiting to start costruction\n";
            return text;
        }
    }
}
