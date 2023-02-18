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

            public override Params CreateParams(IIndustryFacingNodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => $"build {industryFactory.Name}";

            ITooltip IBuildableFactory.CreateTooltip(IIndustryFacingNodeState state)
                => Tooltip(state: state);

            Industry IBuildableFactory.CreateIndustry(IIndustryFacingNodeState state)
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
                => $"""
                {base.TooltipText}
                Construction parameters:
                {nameof(ReqWatts)}: {ReqWatts}
                {nameof(duration)}: {duration}
                {nameof(Cost)}: {Cost}

                Building parameters:
                {industryFactory.CreateParams(state: state).TooltipText}
                """;

            private readonly Factory factory;

            public Params(IIndustryFacingNodeState state, Factory factory)
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
                => Parameters.state.Position;

            public UDouble Radius
                => Parameters.state.Radius + CurWorldConfig.defaultIndustryHeight;
        }

        public override bool PeopleWorkOnTop
            => true;

        public override Propor? SurfaceReflectance
            => donePropor == Propor.empty ? null : parameters.Cost.Reflectance();

        protected override UDouble Height
            => CurWorldConfig.defaultIndustryHeight * donePropor;

        private readonly Params parameters;
        private readonly Disk industryOutline;
        private TimeSpan constrTimeLeft;
        private Propor donePropor;
        private Building? buildingBeingConstructed;

        private Construction(Params parameters)
            : base(parameters: parameters, building: null)
        {
            this.parameters = parameters;
            industryOutline = new(new IndustryOutlineParams(Parameters: parameters));
            constrTimeLeft = TimeSpan.MaxValue;
            donePropor = Propor.empty;
        }

        public override ResAmounts TargetStoredResAmounts()
            => IsBusy().SwitchExpression
            (
                trueCase: () => ResAmounts.Empty,
                falseCase: () => parameters.Cost
            );

        protected override BoolWithExplanationIfFalse IsBusy()
            => base.IsBusy() & BoolWithExplanationIfFalse.Create
            (
                value: StartedConstruction,
                explanationIfFalse: """
                    not enough resources
                    to start construction
                    """
            );

        private bool StartedConstruction
            => constrTimeLeft < TimeSpan.MaxValue;

        protected override Industry InternalUpdate(Propor workingPropor)
        {
            try
            {
                if (!StartedConstruction)
                {
                    var reservedBuildingCost = ResPile.CreateIfHaveEnough
                    (
                        source: parameters.state.StoredResPile,
                        amount: parameters.Cost
                    );
                    if (reservedBuildingCost is not null)
                    {
                        buildingBeingConstructed = new(resSource: reservedBuildingCost);
                        constrTimeLeft = parameters.duration;
                    }
                    return this;
                }

                constrTimeLeft -= workingPropor * CurWorldManager.Elapsed;

                if (constrTimeLeft <= TimeSpan.Zero)
                {
                    constrTimeLeft = TimeSpan.Zero;
                    Debug.Assert(buildingBeingConstructed is not null);
                    Industry newIndustry = parameters.industryFactory.CreateIndustry
                    (
                        state: parameters.state,
                        building: buildingBeingConstructed
                    );
                    buildingBeingConstructed = null;
                    Delete();
                    return newIndustry;
                }
                return this;
            }
            finally
            {
                donePropor = StartedConstruction ? C.DonePropor(timeLeft: constrTimeLeft, duration: parameters.duration) : Propor.empty;
            }
        }

        protected override void PlayerDelete()
        {
            buildingBeingConstructed?.Delete(resDestin: parameters.state.StoredResPile);

            base.PlayerDelete();
        }

        protected override string GetBusyInfo()
            => $"constructing {donePropor * 100.0: 0.}%\n";

        protected override UDouble ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy().SwitchExpression
            (
                trueCase: () => parameters.ReqWatts * CurSkillPropor,
                falseCase: () => (UDouble)0
            );

        public override void DrawBeforePlanet(Color otherColor, Propor otherColorPropor)
        {
            Propor transparency = (Propor).25;
            industryOutline.Draw(baseColor: parameters.industryFactory.Color * (float)transparency, otherColor: otherColor * (float)transparency, otherColorPropor: otherColorPropor * transparency);

            base.DrawBeforePlanet(otherColor, otherColorPropor);
        }
    }
}
