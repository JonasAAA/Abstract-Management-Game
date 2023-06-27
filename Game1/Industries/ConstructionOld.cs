//using Game1.Shapes;
//using Game1.UI;
//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class Construction : ProductiveIndustry
//    {
//        [Serializable]
//        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
//        {
//            public readonly UDouble reqWattsPerUnitSurface;
//            public readonly IFactoryForIndustryWithBuilding industryFactory;
//            public readonly TimeSpan duration;

//            public Factory(string Name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, IFactoryForIndustryWithBuilding industryFactory, TimeSpan duration)
//                : base
//                (
//                    industryType: IndustryType.Construction,
//                    Name: Name,
//                    Color: industryFactory.Color,
//                    energyPriority: energyPriority,
//                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
//                )
//            {
//                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
//                    throw new ArgumentOutOfRangeException();
//                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
//                this.industryFactory = industryFactory;
//                if (duration < TimeSpan.Zero || duration == TimeSpan.MaxValue)
//                    throw new ArgumentException();
//                this.duration = duration;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            string IBuildableFactory.ButtonName
//                => $"build {industryFactory.Name}";

//            ITooltip IBuildableFactory.CreateTooltip(IIndustryFacingNodeState state)
//                => Tooltip(state: state);

//            Industry IBuildableFactory.CreateIndustry(IIndustryFacingNodeState state)
//                => new Construction(parameters: CreateParams(state: state));
//        }

//        [Serializable]
//        public new sealed class GeneralParams : ProductiveIndustry.GeneralParams
//        {
//            public UDouble ReqWatts
//                => state.SurfaceLength * factory.reqWattsPerUnitSurface;
//            public readonly IFactoryForIndustryWithBuilding industryFactory;
//            public readonly TimeSpan duration;
//            public SomeResAmounts<ResAmounts> Cost
//                => industryFactory.BuildingCost(state: state);

//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                Construction parameters:
//                {nameof(ReqWatts)}: {ReqWatts}
//                {nameof(duration)}: {duration}
//                {nameof(Cost)}: {Cost}

//                BuildingShape parameters:
//                {industryFactory.CreateParams(state: state).TooltipText}
//                """;

//            private readonly Factory factory;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//            {
//                this.factory = factory;

//                industryFactory = factory.industryFactory;
//                duration = factory.duration;
//            }
//        }

//        [Serializable]
//        public readonly record struct IndustryOutlineParams(GeneralParams Parameters) : Disk.IParams
//        {
//            public MyVector2 Center
//                => Parameters.state.Position;

//            public UDouble radius
//                => Parameters.state.radius + CurWorldConfig.defaultIndustryHeight;
//        }

//        public override bool PeopleWorkOnTop
//            => true;

//        public override Propor? SurfaceReflectance
//            => donePropor == Propor.empty ? null : parameters.Cost.Reflectance();

//        public override Propor? SurfaceEmissivity
//            => donePropor == Propor.empty ? null : parameters.Cost.Emissivity();

//        protected override UDouble Height
//            => CurWorldConfig.defaultIndustryHeight * donePropor;

//        private readonly GeneralParams parameters;
//        private readonly Disk industryOutline;
//        private TimeSpan constrProporLeft;
//        private Propor donePropor;
//        private BuildingShape? buildingBeingConstructed;

//        private Construction(GeneralParams parameters)
//            : base(parameters: parameters, building: null)
//        {
//            this.parameters = parameters;
//            industryOutline = new(new IndustryOutlineParams(Parameters: parameters));
//            constrProporLeft = TimeSpan.MaxValue;
//            donePropor = Propor.empty;
//        }

//        public override ResAmounts TargetStoredResAmounts()
//            => IsBusy().SwitchExpression
//            (
//                trueCase: () => ResAmounts.empty,
//                falseCase: () => parameters.Cost
//            );

//        protected override BoolWithExplanationIfFalse CalculateIsBusy()
//            => base.CalculateIsBusy() & BoolWithExplanationIfFalse.Create
//            (
//                value: StartedConstruction,
//                explanationIfFalse: """
//                    not enough resources
//                    to start construction
//                    """
//            );

//        private bool StartedConstruction
//            => constrProporLeft < TimeSpan.MaxValue;

//        protected override Industry InternalUpdate(Propor workingPropor)
//        {
//            try
//            {
//                if (!StartedConstruction)
//                {
//                    var reservedBuildingCost = ResPile.CreateIfHaveEnough
//                    (
//                        source: parameters.state.StoredResPile,
//                        amount: parameters.Cost
//                    );
//                    if (reservedBuildingCost is not null)
//                    {
//                        buildingBeingConstructed = new(resSource: reservedBuildingCost);
//                        constrProporLeft = parameters.duration;
//                    }
//                    return this;
//                }

//                constrProporLeft -= workingPropor * CurWorldManager.Elapsed;

//                if (constrProporLeft <= TimeSpan.Zero)
//                {
//                    constrProporLeft = TimeSpan.Zero;
//                    Debug.Assert(buildingBeingConstructed is not null);
//                    Industry newIndustry = parameters.industryFactory.CreateIndustry
//                    (
//                        state: parameters.state,
//                        building: buildingBeingConstructed
//                    );
//                    buildingBeingConstructed = null;
//                    Delete();
//                    return newIndustry;
//                }
//                return this;
//            }
//            finally
//            {
//                donePropor = StartedConstruction ? C.donePropor(timeLeft: constrProporLeft, duration: parameters.duration) : Propor.empty;
//            }
//        }

//        protected override void PlayerDelete()
//        {
//            buildingBeingConstructed?.Delete(resDestin: parameters.state.StoredResPile);

//            base.PlayerDelete();
//        }

//        protected override string GetBusyInfo()
//            => $"constructing {donePropor * 100.0: 0.}%\n";

//        protected override UDouble ReqWatts()
//            // this is correct as if more important people get full energy, this works
//            // and if they don't, then the industry will get 0 energy anyway
//            => IsBusy().SwitchExpression
//            (
//                trueCase: () => parameters.ReqWatts * CurSkillPropor,
//                falseCase: () => UDouble.zero
//            );

//        public override void Draw(Color otherColor, Propor otherColorPropor)
//        {
//            Propor transparency = (Propor).25;
//            industryOutline.Draw(baseColor: parameters.industryFactory.Color * (float)transparency, otherColor: otherColor * (float)transparency, otherColorPropor: otherColorPropor * transparency);

//            base.Draw(otherColor, otherColorPropor);
//        }
//    }
//}
