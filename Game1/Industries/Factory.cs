using System;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class Factory : BuildingIndustry
    {
        [Serializable]
        public new class Params : BuildingIndustry.Params
        {
            public readonly double reqWatts;
            public readonly ConstULongArray supply, demand;
            public readonly TimeSpan prodDuration;

            public Params(string name, ulong energyPriority, double reqSkill, ulong reqWatts, ConstULongArray supply, ConstULongArray demand, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Production,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkill: reqSkill,
                    explanation: $"requires {reqSkill} skill\nrequires {reqWatts} W\nsupply {supply}\ndemand {demand}"
                )
            {
                if (reqWatts <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWatts = reqWatts;
                this.supply = supply;
                this.demand = demand;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            public override Factory MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private TimeSpan prodTimeLeft;

        private Factory(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
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

        protected override Industry Update(double workingPropor)
        {
            if (IsBusy())
                prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;

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

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"producing {C.DonePart(timeLeft: prodTimeLeft, duration: parameters.prodDuration) * 100: 0.}%\n";
            else
                text += "idle\n";
            if (!CanStartProduction)
                text += "will not start new\n";
            return text;
        }

        public override double ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy() switch
            {
                true => parameters.reqWatts * CurSkillPropor,
                false => 0
            };
    }
}