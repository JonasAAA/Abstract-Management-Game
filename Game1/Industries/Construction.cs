using Game1.ChangingValues;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Construction : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly Industry.Factory industryFactory;
            public readonly TimeSpan duration;
            public readonly ResAmounts costPerUnitSurface;
            
            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, Industry.Factory industryFactory, TimeSpan duration, ResAmounts costPerUnitSurface)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"construction stats:\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(duration)} {duration.TotalSeconds:0.}s\n{nameof(costPerUnitSurface)} {costPerUnitSurface}\n\nbuilding stats:\n{industryFactory.Explanation}"
                )
            {
                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.industryFactory = industryFactory;
                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
                    throw new ArgumentException();
                this.duration = duration;
                this.costPerUnitSurface = costPerUnitSurface;
            }

            protected override Params CreateParams(NodeState state)
                => new
                (
                    baseParams: base.CreateParams(state: state),
                    reqWatts: state.approxSurfaceLength * reqWattsPerUnitSurface,
                    industryFactory: industryFactory,
                    duration: duration,
                    cost: state.approxSurfaceLength * costPerUnitSurface
                );

            public override Construction CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            string IBuildableFactory.ButtonName
                => $"build {industryFactory.name}";
        }

        [Serializable]
        public new sealed record Params : ProductiveIndustry.Params
        {
            public readonly IReadOnlyChangingUDouble reqWatts;
            public readonly Industry.Factory industryFactory;
            public readonly TimeSpan duration;
            public readonly IReadOnlyChangingResAmounts cost;

            public Params(ProductiveIndustry.Params baseParams, IReadOnlyChangingUDouble reqWatts, Industry.Factory industryFactory, TimeSpan duration, IReadOnlyChangingResAmounts cost)
                : base(baseParams)
            {
                this.reqWatts = reqWatts;
                this.industryFactory = industryFactory;
                this.duration = duration;
                this.cost = cost;
            }
        }

        private readonly Params parameters;
        private TimeSpan constrTimeLeft;

        private Construction(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ResAmounts TargetStoredResAmounts()
            => IsBusy() switch
            {
                true => new(),
                false => parameters.cost.Value,
            };

        protected override bool IsBusy()
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry InternalUpdate(Propor workingPropor)
        {
            if (IsBusy())
                constrTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (!IsBusy() && parameters.state.storedRes >= parameters.cost.Value)
            {
                parameters.state.storedRes -= parameters.cost.Value;
                constrTimeLeft = parameters.duration;
            }

            if (constrTimeLeft <= TimeSpan.Zero)
            {
                constrTimeLeft = TimeSpan.Zero;
                Delete();
                return parameters.industryFactory.CreateIndustry(state: parameters.state);
            }
            return this;
        }

        protected override void PlayerDelete()
        {
            base.PlayerDelete();

            parameters.state.storedRes += IsBusy() switch
            {
                true => parameters.cost.Value / 2,
                false => parameters.cost.Value
            };
        }

        public override string GetInfo()
        {
            string text = base.GetInfo();
            if (IsBusy())
                text += $"constructing {C.DonePropor(timeLeft: constrTimeLeft, duration: parameters.duration) * 100.0: 0.}%\n";
            else
                text += "waiting to start costruction\n";
            return text;
        }

        public override UDouble ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy() switch
            {
                true => parameters.reqWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}
