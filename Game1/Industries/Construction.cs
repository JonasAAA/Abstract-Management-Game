using Game1.UI;
using Microsoft.Xna.Framework;
using System;

namespace Game1.Industries
{
    public class Construction : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly double reqWattsPerSec;
            public readonly Industry.Params industrParams;
            public readonly TimeSpan duration;
            public readonly ConstULongArray cost;

            public Params(string name, ulong electrPriority, double reqSkill, ulong reqWattsPerSec, Industry.Params industrParams, TimeSpan duration, ConstULongArray cost)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    electrPriority: electrPriority,
                    reqSkill: reqSkill,
                    explanation: $"construction stats:\nrequires {reqWattsPerSec} W/s\nduration {duration.TotalSeconds:0.}s\ncost {cost}\n\nbuilding stats:\n{industrParams.explanation}"
                )
            {
                if (reqWattsPerSec <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerSec = reqWattsPerSec;
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
            : base
            (
                parameters: parameters,
                state: state,
                UIPanel: new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
            )
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
                constrTimeLeft -= workingPropor * Graph.Elapsed;

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

        public override string GetText()
        {
            string text = base.GetText();
            if (IsBusy())
                text += $"constructing {C.DonePart(timeLeft: constrTimeLeft, duration: parameters.duration) * 100: 0.}%\n";
            else
                text += "waiting to start costruction\n";
            return text;
        }

        public override double ReqWattsPerSec()
            // this is correct as if more important people get full electricity, this works
            // and if they don't, then the industry will get 0 electricity anyway
            => IsBusy() switch
            {
                true => parameters.reqWattsPerSec * CurSkillPropor,
                false => 0
            };
    }
}
