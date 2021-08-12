using Microsoft.Xna.Framework.Input;
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
            public readonly ulong reqWattsPerSec;

            public Params(string name, Industry.Params industrParams, TimeSpan duration, ConstULongArray cost, ulong reqWattsPerSec)
                : base(industryType: IndustryType.Construction, name: name)
            {
                this.industrParams = industrParams;
                if (duration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
                if (reqWattsPerSec < 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerSec = reqWattsPerSec;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Construction(parameters: this, state: state);
        }

        private readonly Params parameters;
        private TimeSpan? constrEndTime;

        private Construction(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            constrEndTime = null;
        }

        public override ULongArray TargetStoredResAmounts()
            => parameters.cost.ToULongArray();

        public override ulong ReqWattsPerSec()
        {
            if (constrEndTime.HasValue)
                return parameters.reqWattsPerSec;
            return 0;
        }

        public override ulong ProdWattsPerSec()
            => 0;

        public override Industry Update()
        {
            if (constrEndTime is null && state.storedRes >= parameters.cost)
            {
                constrEndTime = C.TotalGameTime + parameters.duration;
                state.storedRes -= parameters.cost;
            }

            if (constrEndTime.HasValue && constrEndTime <= C.TotalGameTime)
                return parameters.industrParams.MakeIndustry(state: state);
            return this;
        }

        public override string GetText()
            => constrEndTime switch
            {
                null => "waiting to start costruction",
                not null => $"constructing {C.DonePart(endTime: constrEndTime.Value, duration: parameters.duration) * 100: 0.}%"
            };
    }
}
