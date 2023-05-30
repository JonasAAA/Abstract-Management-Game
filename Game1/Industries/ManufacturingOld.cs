//using System.Diagnostics.CodeAnalysis;
//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class Manufacturing : ProductiveIndustry
//    {
//        [Serializable]
//        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
//        {
//            public readonly UDouble reqWattsPerUnitSurface;
//            public readonly TimeSpan prodDuration;
//            public readonly ResRecipe baseResRecipe;
//            public readonly ulong prodResPerUnitSurface;
//            private readonly ResAmounts buildingCostPerUnitSurface;

//            public Factory(string Name, ResRecipe baseResRecipe, ulong prodResPerUnitSurface, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, TimeSpan prodDuration, ResAmounts buildingCostPerUnitSurface)
//                : base
//                (
//                    industryType: IndustryType.Manufacturing,
//                    Name: Name,
//                    color: Color.Brown,
//                    energyPriority: energyPriority,
//                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
//                )
//            {
//                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
//                    throw new ArgumentOutOfRangeException();
//                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
//                if (prodDuration < TimeSpan.Zero)
//                    throw new ArgumentException();
//                this.prodDuration = prodDuration;
//                if (baseResRecipe.IsEmpty)
//                    throw new ArgumentException();
//                this.baseResRecipe = baseResRecipe;
//                if (prodResPerUnitSurface == 0)
//                    throw new ArgumentException();
//                this.prodResPerUnitSurface = prodResPerUnitSurface;
//                if (buildingCostPerUnitSurface.IsEmpty())
//                    throw new ArgumentException();
//                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            ResAmounts IFactoryForIndustryWithBuilding.BuildingCost(IIndustryFacingNodeState state)
//                => state.SurfaceLength * buildingCostPerUnitSurface;

//            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, Building building)
//                => new Manufacturing(parameters: CreateParams(state: state), building: building);
//        }

//        [Serializable]
//        public new sealed class GeneralParams : ProductiveIndustry.GeneralParams
//        {
//            public UDouble ReqWatts
//                => state.SurfaceLength * factory.reqWattsPerUnitSurface;
//            public readonly TimeSpan prodDuration;
//            public ResRecipe ResRecipe
//                => state.SurfaceLength * factory.prodResPerUnitSurface * factory.baseResRecipe;

//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                {nameof(ReqWatts)}: {ReqWatts}
//                {nameof(prodDuration)}: {prodDuration}
//                Supply: {ResRecipe.results}
//                Demand: {ResRecipe.ingredients}
//                """;

//            private readonly Factory factory;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//            {
//                this.factory = factory;

//                prodDuration = factory.prodDuration;
//            }
//        }

//        [Serializable]
//        private class Production
//        {
//            private static Production? Create(ResPile source, ResRecipe recipe, TimeSpan duration)
//            {
//                if (duration <= TimeSpan.Zero)
//                    throw new ArgumentException();
//                var resInUse = ResPile.CreateIfHaveEnough(source: source, amount: recipe.ingredients);
//                if (resInUse is null)
//                    return null;
//                return new(resInUse: resInUse, recipe: recipe, duration: duration);
//            }

//            public static void Update(ref Production? product, GeneralParams parameters, Propor workingPropor)
//            {
//                if (product is null)
//                {
//                    product = Create
//                    (
//                        source: parameters.state.StoredResPile,
//                        recipe: parameters.ResRecipe,
//                        duration: parameters.prodDuration
//                    );
//                    return;
//                }
//                product.prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;
//                if (product.prodTimeLeft <= TimeSpan.Zero)
//                {
//                    parameters.state.StoredResPile.TransformFrom(source: product.resInUse, recipe: product.recipe);
//                    product = null;
//                }
//            }

//            private readonly ResPile resInUse;
//            private readonly ResRecipe recipe;
//            private TimeSpan prodTimeLeft;
//            private readonly TimeSpan duration;

//            private Production(ResPile resInUse, ResRecipe recipe, TimeSpan duration)
//            {
//                this.resInUse = resInUse;
//                this.recipe = recipe;
//                this.duration = duration;
//                prodTimeLeft = duration;
//            }

//            public Propor donePropor()
//                => C.donePropor(timeLeft: prodTimeLeft, duration: duration);
//        }

//        public override bool PeopleWorkOnTop
//            => false;

//        protected sealed override UDouble Height
//            => CurWorldConfig.defaultIndustryHeight;

//        private readonly GeneralParams parameters;
//        private Production? production;

//        private Manufacturing(GeneralParams parameters, Building building)
//            : base(parameters: parameters, building: building)
//        {
//            this.parameters = parameters;
//            production = null;
//        }

//        protected override BoolWithExplanationIfFalse CalculateIsBusy()
//            => base.CalculateIsBusy() & BoolWithExplanationIfFalse.Create
//            (
//                value: production is not null,
//                explanationIfFalse: """
//                    not enough resources
//                    to start manufacturing

//                    """
//            );

//        protected override Manufacturing InternalUpdate(Propor workingPropor)
//        {
//            Production.Update(product: ref production, parameters: parameters, workingPropor: workingPropor);
//            return this;
//        }

//        public override ResAmounts TargetStoredResAmounts()
//            => parameters.ResRecipe.ingredients * parameters.state.MaxBatchDemResStored;

//        protected override string GetBusyInfo()
//            => $"producing {production!.donePropor() * 100.0: 0.}%\n";

//        protected override UDouble ReqWatts()
//            // this is correct as if more important people get full energy, this works
//            // and if they don't, then the industry will get 0 energy anyway
//            => IsBusy().SwitchExpression
//            (
//                trueCase: () => parameters.ReqWatts * CurSkillPropor,
//                falseCase: () => UDouble.zero
//            );
//    }
//}
