using System;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class Construction : ProductiveIndustry
    {
        [Serializable]
        public new class Params : ProductiveIndustry.Params
        {
            public readonly double reqWatts;
            public readonly Industry.Params industrParams;
            public readonly TimeSpan duration;
            public readonly ConstULongArray cost;

            public Params(string name, ulong energyPriority, double reqSkill, ulong reqWatts, Industry.Params industryParams, TimeSpan duration, ConstULongArray cost)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkill: reqSkill,
                    explanation: $"construction stats:\nrequires {reqWatts} W/s\nduration {duration.TotalSeconds:0.}s\ncost {cost}\n\nbuilding stats:\n{industryParams.explanation}"
                )
            {
                if (reqWatts <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWatts = reqWatts;
                this.industrParams = industryParams;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
            }

            public override Construction MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private TimeSpan constrTimeLeft;

        private Construction(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ULongArray TargetStoredResAmounts()
            => IsBusy() switch
            {
                true => new(),
                false => parameters.cost.ToULongArray(),
            };

        protected override bool IsBusy()
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry InternalUpdate(double workingPropor)
        {
            if (IsBusy())
                constrTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (!IsBusy() && state.storedRes >= parameters.cost)
            {
                state.storedRes -= parameters.cost;
                constrTimeLeft = parameters.duration;
            }

            if (constrTimeLeft <= TimeSpan.Zero)
            {
                Delete();
                return parameters.industrParams.MakeIndustry(state: state);
            }
            return this;
        }

        protected override void PlayerDelete()
        {
            base.PlayerDelete();

            state.storedRes += IsBusy() switch
            {
                true => parameters.cost / 2,
                false => parameters.cost
            };
        }

        public override string GetInfo()
        {
            string text = base.GetInfo();
            if (IsBusy())
                text += $"constructing {C.DonePart(timeLeft: constrTimeLeft, duration: parameters.duration) * 100: 0.}%\n";
            else
                text += "waiting to start costruction\n";
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
