using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [DataContract]
    public class Factory : Industry
    {
        [DataContract]
        public new class Params : Industry.Params
        {
            [DataMember]
            public readonly double reqWatts;
            [DataMember]
            public readonly ConstULongArray supply, demand;
            [DataMember]
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

            protected override Industry MakeIndustry(NodeState state)
                => new Factory(state: state, parameters: this);
        }

        [DataMember]
        private readonly Params parameters;
        [DataMember]
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
                prodTimeLeft -= workingPropor * Elapsed;

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