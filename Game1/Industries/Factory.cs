using Game1.UI;
using Microsoft.Xna.Framework;
using System;

namespace Game1.Industries
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly double reqWattsPerSec;
            public readonly ConstULongArray supply, demand;
            public readonly TimeSpan prodDuration;

            public Params(string name, ulong electrPriority, double reqSkill, ulong reqWattsPerSec, ConstULongArray supply, ConstULongArray demand, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Production,
                    name: name,
                    electrPriority: electrPriority,
                    reqSkill: reqSkill,
                    explanation: $"requires {reqSkill} skill\nrequires {reqWattsPerSec} W/s\nsupply {supply}\ndemand {demand}"
                )
            {
                if (reqWattsPerSec <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerSec = reqWattsPerSec;
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
            : base
            (
                parameters: parameters,
                state: state,
                UIPanel: new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
            )
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
                prodTimeLeft -= workingPropor * Graph.Elapsed;

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