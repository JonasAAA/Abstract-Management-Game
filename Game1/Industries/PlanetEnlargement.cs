//using Game1.Shapes;
//using Game1.UI;
//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class PlanetEnlargement : ProductiveIndustry
//    {
//        [Serializable]
//        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
//        {
//            public readonly UDouble reqWattsPerUnitSurface;
//            public readonly UDouble addedResPerUnitSurfacePerSec;

//            public Factory(string Name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, UDouble addedResPerUnitSurfacePerSec)
//                : base
//                (
//                    industryType: IndustryType.PlanetEnlargement,
//                    Name: Name,
//                    Color: Color.Pink,
//                    energyPriority: energyPriority,
//                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
//                )
//            {
//                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
//                this.addedResPerUnitSurfacePerSec = addedResPerUnitSurfacePerSec;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            string IBuildableFactory.ButtonName
//                => Name;

//            ITooltip IBuildableFactory.CreateTooltip(IIndustryFacingNodeState state)
//                => Tooltip(state: state);

//            Industry IBuildableFactory.CreateIndustry(IIndustryFacingNodeState state)
//                => new PlanetEnlargement(parameters: CreateParams(state: state));
//        }

//        [Serializable]
//        public new sealed class GeneralParams : ProductiveIndustry.GeneralParams
//        {
//            public UDouble ReqWatts
//                => state.SurfaceLength * factory.reqWattsPerUnitSurface;
//            public UDouble AddedResPerSec
//                => state.SurfaceLength * factory.addedResPerUnitSurfacePerSec;

//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                {nameof(ReqWatts)}: {ReqWatts}
//                {nameof(AddedResPerSec)}: {AddedResPerSec}
//                """;

//            private readonly Factory factory;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//            {
//                this.factory = factory;
//            }
//        }

//        [Serializable]
//        private readonly record struct FutureShapeOutlineParams(GeneralParams Parameters) : Ring.IParamsWithInnerRadius
//        {
//            public MyVector2 Center
//                => Parameters.state.Position;

//            public UDouble InnerRadius
//                => Parameters.state.radius;
//        }

//        public override bool PeopleWorkOnTop
//            => true;

//        protected override UDouble Height
//            => 0;

//        private readonly GeneralParams parameters;
//        /// <summary>
//        /// Since each frame a non-integer amount will be added, and resources can only be moved in integer amounts,
//        /// this represents the amount that is added, but not counted yet. Must be between 0 and 1.
//        /// </summary>
//        private UDouble silentlyAddedBits;
//        private UDouble curAddedResPerSec;
//        private readonly InnerRing futureShapeOutline;

//        private PlanetEnlargement(GeneralParams parameters)
//            : base(parameters: parameters, building: null)
//        {
//            this.parameters = parameters;
//            silentlyAddedBits = 0;
//            curAddedResPerSec = 0;
//            futureShapeOutline = new(parameters: new FutureShapeOutlineParams(Parameters: parameters));
//        }

//        public override ResAmounts TargetStoredResAmounts()
//            => new
//            (
//                resInd: parameters.state.ConsistsOf,
//                amount: (ulong)(parameters.AddedResPerSec * 60)
//            );

//        protected override PlanetEnlargement InternalUpdate(Propor workingPropor)
//        {
//            UDouble targetAddedRes = workingPropor * parameters.AddedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds,
//                resToAdd = targetAddedRes + silentlyAddedBits;
//            ulong addedRes = (ulong)resToAdd;
//            silentlyAddedBits = (UDouble)(resToAdd - addedRes);
//            Debug.Assert(0 <= silentlyAddedBits && silentlyAddedBits <= 1);

//            ulong maxAddedRes = parameters.state.StoredResPile.Amount[parameters.state.ConsistsOf];
//            if (addedRes > maxAddedRes)
//            {
//                addedRes = maxAddedRes;
//                silentlyAddedBits = 0;
//            }

//            curAddedResPerSec = MyMathHelper.Min(targetAddedRes, maxAddedRes) / (UDouble)CurWorldManager.Elapsed.TotalSeconds;
//            parameters.state.EnlargeFrom(source: parameters.state.StoredResPile, resAmount: addedRes);

//            return this;
//        }

//        protected override string GetBusyInfo()
//            => $"Adding {curAddedResPerSec:0.##} {parameters.state.ConsistsOf} per second\n";

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
//            base.Draw(otherColor, otherColorPropor);

//            futureShapeOutline.Draw(baseColor: parameters.state.ConsistsOf.Color, otherColor: otherColor, otherColorPropor: otherColorPropor);
//        }
//    }
//}
