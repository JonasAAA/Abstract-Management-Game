﻿using Game1.Shapes;
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
            public readonly IFactoryForIndustryWithBuilding industryFactory;
            public readonly TimeSpan duration;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, IFactoryForIndustryWithBuilding industryFactory, TimeSpan duration)
                : base
                (
                    industryType: IndustryType.Construction,
                    name: name,
                    color: industryFactory.Color,
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
            }

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => $"build {industryFactory.Name}";

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);

            Industry IBuildableFactory.CreateIndustry(NodeState state)
                => new Construction(parameters: CreateParams(state: state));
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public readonly IFactoryForIndustryWithBuilding industryFactory;
            public readonly TimeSpan duration;
            public ResAmounts Cost
                => industryFactory.BuildingCost(state: state);

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
        private readonly ResPile resInBuilding;

        private Construction(Params parameters)
            : base(parameters: parameters, building: null)
        {
            this.parameters = parameters;
            industryOutline = new(new IndustryOutlineParams(Parameters: parameters));
            constrTimeLeft = TimeSpan.MaxValue;
            resInBuilding = ResPile.CreateEmpty();
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

                if (!IsBusy() && parameters.state.storedResPile.ResAmounts >= parameters.Cost)
                {
                    ResPile.Transfer(source: parameters.state.storedResPile, destin: resInBuilding, resAmounts: parameters.Cost);
                    constrTimeLeft = parameters.duration;
                }

                if (constrTimeLeft <= TimeSpan.Zero)
                {
                    constrTimeLeft = TimeSpan.Zero;
                    Delete();
                    return parameters.industryFactory.CreateIndustry
                    (
                        state: parameters.state,
                        building: new
                        (
                            resSource: resInBuilding,
                            cost: parameters.industryFactory.BuildingCost(state: parameters.state)
                        )
                    );
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
            ResPile.TransferAll(source: resInBuilding, destin: parameters.state.storedResPile);

            base.PlayerDelete();
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
            industryOutline.Draw(baseColor: parameters.industryFactory.Color * (float)transparency, otherColor: otherColor * (float)transparency, otherColorPropor: otherColorPropor * transparency);

            base.DrawBeforePlanet(otherColor, otherColorPropor);
        }
    }
}
