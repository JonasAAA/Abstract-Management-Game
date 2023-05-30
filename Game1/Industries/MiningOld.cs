//using Game1.Shapes;
//using Game1.UI;
//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class Mining : ProductiveIndustry
//    {
//        [Serializable]
//        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
//        {
//            public readonly UDouble reqWattsPerUnitSurface;
//            public readonly UDouble minedResPerUnitSurfacePerSec;

//            public Factory(string Name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, UDouble minedResPerUnitSurfacePerSec)
//                : base
//                (
//                    industryType: IndustryType.Mining,
//                    Name: Name,
//                    color: Color.Pink,
//                    energyPriority: energyPriority,
//                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
//                )
//            {
//                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
//                this.minedResPerUnitSurfacePerSec = minedResPerUnitSurfacePerSec;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            string IBuildableFactory.ButtonName
//                => Name;

//            ITooltip IBuildableFactory.CreateTooltip(IIndustryFacingNodeState state)
//                => Tooltip(state: state);

//            Industry IBuildableFactory.CreateIndustry(IIndustryFacingNodeState state)
//                => new Mining(parameters: CreateParams(state: state));
//        }

//        [Serializable]
//        public new sealed class GeneralParams : ProductiveIndustry.GeneralParams
//        {
//            public UDouble ReqWatts
//                => state.SurfaceLength * factory.reqWattsPerUnitSurface;
//            public UDouble MinedResPerSec
//                => state.SurfaceLength * factory.minedResPerUnitSurfacePerSec;

//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                {nameof(ReqWatts)}: {ReqWatts}
//                {nameof(MinedResPerSec)}: {MinedResPerSec}
//                """;

//            private readonly Factory factory;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//            {
//                this.factory = factory;
//            }
//        }

//        [Serializable]
//        private readonly record struct FutureShapeOutlineParams(GeneralParams Parameters) : Ring.IParamsWithOuterRadius
//        {
//            public MyVector2 Center
//                => Parameters.state.Position;

//            public UDouble OuterRadius
//                => Parameters.state.Radius + 1;
//        }

//        public override bool PeopleWorkOnTop
//            => true;

//        protected override UDouble Height
//            => 0;

//        private readonly GeneralParams parameters;
//        /// <summary>
//        /// Since each frame a non-integer amount will be mined, and resources can only be moved in integer amounts,
//        /// this represents the amount that is mined, but not counted yet. Must be between 0 and 1.
//        /// </summary>
//        private UDouble silentlyMinedBits;
//        private UDouble minedResPerSec;
//        private readonly OuterRing futureShapeOutline;

//        private Mining(GeneralParams parameters)
//            : base(parameters: parameters, building: null)
//        {
//            this.parameters = parameters;
//            silentlyMinedBits = 0;
//            minedResPerSec = 0;
//            futureShapeOutline = new(parameters: new FutureShapeOutlineParams(Parameters: parameters));
//        }

//        protected override BoolWithExplanationIfFalse CalculateIsBusy()
//            => base.CalculateIsBusy() & BoolWithExplanationIfFalse.Create
//            (
//                value: parameters.state.MaxAvailableResAmount > 0,
//                explanationIfFalse: "the planet is fully mined out\n"
//            );

//        public override ResAmounts TargetStoredResAmounts()
//            => ResAmounts.empty;

//        protected override Mining InternalUpdate(Propor workingPropor)
//        {
//            UDouble targetMinedRes = workingPropor * parameters.MinedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds,
//                resToMine = targetMinedRes + silentlyMinedBits;
//            ulong minedRes = (ulong)resToMine;
//            silentlyMinedBits = (UDouble)(resToMine - minedRes);
//            Debug.Assert(0 <= silentlyMinedBits && silentlyMinedBits <= 1);

//            ulong maxMinedRes = parameters.state.MaxAvailableResAmount;
//            if (minedRes > maxMinedRes)
//            {
//                minedRes = maxMinedRes;
//                silentlyMinedBits = 0;
//            }

//            minedResPerSec = MyMathHelper.Min(targetMinedRes, maxMinedRes) / (UDouble)CurWorldManager.Elapsed.TotalSeconds;
//            parameters.state.MineTo(destin: parameters.state.StoredResPile, resAmount: minedRes);

//            return this;
//        }

//        protected override string GetBusyInfo()
//            => $"Mining {minedResPerSec:0.##} {parameters.state.ConsistsOf} per second\n";

//        // TODO: get rid of the duplication of this method code
//        protected override UDouble ReqWatts()
//            // this is correct as if more important people get full energy, this works
//            // and if they don't, then the industry will get 0 energy anyway
//            => IsBusy().SwitchExpression
//            (
//                trueCase: () => parameters.ReqWatts * CurSkillPropor,
//                falseCase: () => UDouble.zero
//            );

//        public override void DrawAfterPlanet()
//        {
//            base.DrawAfterPlanet();

//            futureShapeOutline.Draw(color: Color.Black);
//        }
//    }
//}
