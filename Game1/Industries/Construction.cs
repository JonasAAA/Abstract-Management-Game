using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [DataContract]
    public class Construction : Industry
    {
        [DataContract]
        public new class Params : Industry.Params
        {
            [DataMember]
            public readonly double reqWatts;
            [DataMember]
            public readonly Industry.Params industrParams;
            [DataMember]
            public readonly TimeSpan duration;
            [DataMember]
            public readonly ConstULongArray cost;

            public Params(string name, ulong energyPriority, double reqSkill, ulong reqWatts, Industry.Params industrParams, TimeSpan duration, ConstULongArray cost)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkill: reqSkill,
                    explanation: $"construction stats:\nrequires {reqWatts} W/s\nduration {duration.TotalSeconds:0.}s\ncost {cost}\n\nbuilding stats:\n{industrParams.explanation}"
                )
            {
                if (reqWatts <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWatts = reqWatts;
                this.industrParams = industrParams;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
            }

            protected override Industry MakeIndustry(NodeState state)
                => new Construction(state: state, parameters: this);
        }

        [DataMember]
        private readonly Params parameters;
        [DataMember]
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

        protected override Industry Update(double workingPropor)
        {
            if (IsBusy())
                constrTimeLeft -= workingPropor * Elapsed;

            if (!IsBusy() && state.storedRes >= parameters.cost)
            {
                state.storedRes -= parameters.cost;
                constrTimeLeft = parameters.duration;
            }

            if (constrTimeLeft <= TimeSpan.Zero)
            {
                Delete();
                return parameters.industrParams.MakeAndInitIndustry(state: state);
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

        public override string GetText()
        {
            string text = base.GetText();
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
