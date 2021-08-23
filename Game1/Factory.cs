using System;

namespace Game1
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly ConstULongArray supply, demand;
            public readonly TimeSpan prodDuration;

            public Params(string name, double reqSkill, ulong reqWattsPerSec, ConstULongArray supply, ConstULongArray demand, TimeSpan prodDuration)
                : base(industryType: IndustryType.Production, name: name, reqSkill: reqSkill, reqWattsPerSec: reqWattsPerSec, prodWattsPerSec: 0)
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
        private TimeSpan? prodEndTime;

        private Factory(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            prodEndTime = null;
        }

        public override ULongArray TargetStoredResAmounts()
        {
            if (CanStartProduction)
                return parameters.demand * state.maxBatchDemResStored;
            return new();
        }

        protected override bool IsBusy()
            => prodEndTime.HasValue;

        public override Industry Update()
        {
            base.Update();

            if (prodEndTime.HasValue)
                prodEndTime += (1 - CurSkillPropor) * C.ElapsedGameTime;

            if (CanStartProduction && prodEndTime is null && state.storedRes >= parameters.demand)
            {
                state.storedRes -= parameters.demand;
                prodEndTime = C.TotalGameTime + parameters.prodDuration;
            }

            if (prodEndTime.HasValue && prodEndTime <= C.TotalGameTime)
            {
                state.waitingTravelPacket.Add(resAmounts: parameters.supply);
                prodEndTime = null;
            }

            return this;
        }

        public override string GetText()
        {
            string text = base.GetText() + $"{parameters.name}\n";
            if (prodEndTime is null)
                text += "idle";
            else
                text += $"producing {C.DonePart(endTime: prodEndTime.Value, duration: parameters.prodDuration) * 100: 0.}%";
            if (!CanStartProduction)
                text += "\nwill not start new";
            return text;
        }
    }
}