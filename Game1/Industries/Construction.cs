using Game1.PrimitiveTypeWrappers;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class Construction : ProductiveIndustry
    {
        [Serializable]
        public new class Params : ProductiveIndustry.Params
        {
            public readonly UFloat reqWattsPerUnitSurface;
            public readonly Industry.Params industryParams;
            public readonly TimeSpan duration;
            public readonly ConstULongArray cost;
            
            // TODO: make cost depend on the planet size
            public Params(string name, ulong energyPriority, UFloat reqSkillPerUnitSurface, UFloat reqWattsPerUnitSurface, Industry.Params industryParams, TimeSpan duration, ConstULongArray cost)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"construction stats:\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(duration)} {duration.TotalSeconds:0.}s\n{nameof(cost)} {cost}\n\nbuilding stats:\n{industryParams.explanation}"
                )
            {
                if (reqWattsPerUnitSurface <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.industryParams = industryParams;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
            }

            public override Construction MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingUFloat reqWatts;
        private TimeSpan constrTimeLeft;

        private Construction(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            reqWatts = parameters.reqWattsPerUnitSurface * state.approxSurfaceLength;
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ConstULongArray TargetStoredResAmounts()
            => IsBusy() switch
            {
                true => new(),
                false => parameters.cost,
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
                return parameters.industryParams.MakeIndustry(state: state);
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
                true => reqWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}
