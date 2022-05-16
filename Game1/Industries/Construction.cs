using Game1.Shapes;
using Game1.UI;
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
                    color: industryFactory.color,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
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

            public override Construction CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => $"build {industryFactory.name}";

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public readonly Industry.Factory industryFactory;
            public readonly TimeSpan duration;
            public ResAmounts Cost
                => state.ApproxSurfaceLength * factory.costPerUnitSurface;

            public override string TooltipText
                => base.TooltipText + $"Construction parameters:\n{nameof(ReqWatts)}: {ReqWatts}\n{nameof(duration)}: {duration}\n{nameof(Cost)}: {Cost}\n\nBuilding paramerers:\n{industryFactory.CreateParams(state: state).TooltipText}";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                industryFactory = factory.industryFactory;
                duration = factory.duration;
            }
        }

        [Serializable]
        public readonly record struct IndustryOutlineParams(Params Parameters) : Disk.IParams
        {
            public MyVector2 Center
                => Parameters.state.position;

            public UDouble Radius
                => Parameters.state.Radius + CurWorldConfig.defaultIndustryHeight;
        }

        public override bool PeopleWorkOnTop
            => true;

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight * donePropor;

        private readonly Params parameters;
        private readonly Disk industryOutline;
        private TimeSpan constrTimeLeft;
        private Propor donePropor;

        private Construction(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            industryOutline = new(new IndustryOutlineParams(Parameters: parameters));
            constrTimeLeft = TimeSpan.MaxValue;
        }

        public override ResAmounts TargetStoredResAmounts()
            => IsBusy() switch
            {
                true => new(),
                false => parameters.Cost,
            };

        protected override bool IsBusy()
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry InternalUpdate(Propor workingPropor)
        {
            try
            {
                if (IsBusy())
                    constrTimeLeft -= workingPropor * CurWorldManager.Elapsed;

                if (!IsBusy() && parameters.state.storedRes >= parameters.Cost)
                {
                    parameters.state.storedRes -= parameters.Cost;
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
            finally
            {
                donePropor = C.DonePropor(timeLeft: constrTimeLeft, duration: parameters.duration);
            }
        }

        protected override void PlayerDelete()
        {
            base.PlayerDelete();

            parameters.state.storedRes += IsBusy() switch
            {
                true => parameters.Cost / 2,
                false => parameters.Cost
            };
        }

        public override string GetInfo()
        {
            string text = base.GetInfo();
            if (IsBusy())
                text += $"constructing {donePropor * 100.0: 0.}%\n";
            else
                text += "waiting to start costruction\n";
            return text;
        }

        public override UDouble ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy() switch
            {
                true => parameters.ReqWatts * CurSkillPropor,
                false => 0
            };

        public override void DrawBeforePlanet(Color otherColor, Propor otherColorPropor)
        {
            Propor transparency = (Propor).25;
            industryOutline.Draw(baseColor: parameters.industryFactory.color * (float)transparency, otherColor: otherColor * (float)transparency, otherColorPropor: otherColorPropor * transparency);

            base.DrawBeforePlanet(otherColor, otherColorPropor);
        }
    }
}
