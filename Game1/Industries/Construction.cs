using Game1.ChangingValues;
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
            public readonly ReadOnlyULongArray costPerUnitSurface;
            
            public Params(string name, EnergyPriority energyPriority, UFloat reqSkillPerUnitSurface, UFloat reqWattsPerUnitSurface, Industry.Params industryParams, TimeSpan duration, ReadOnlyULongArray costPerUnitSurface)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"construction stats:\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(duration)} {duration.TotalSeconds:0.}s\n{nameof(costPerUnitSurface)} {costPerUnitSurface}\n\nbuilding stats:\n{industryParams.explanation}"
                )
            {
                if (reqWattsPerUnitSurface <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.industryParams = industryParams;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.costPerUnitSurface = costPerUnitSurface;
            }

            public override Construction MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingUFloat reqWatts;
        private readonly IReadOnlyChangingULongArray cost;
        private TimeSpan constrTimeLeft;

        private Construction(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            reqWatts = parameters.reqWattsPerUnitSurface * state.approxSurfaceLength;
            cost = parameters.costPerUnitSurface * state.approxSurfaceLength;
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ReadOnlyULongArray TargetStoredResAmounts()
            => IsBusy() switch
            {
                true => new(),
                false => cost.Value,
            };

        protected override bool IsBusy()
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry InternalUpdate(double workingPropor)
        {
            if (IsBusy())
                constrTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (!IsBusy() && state.storedRes >= cost.Value)
            {
                state.storedRes -= cost.Value;
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
                true => cost.Value / 2,
                false => cost.Value
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
