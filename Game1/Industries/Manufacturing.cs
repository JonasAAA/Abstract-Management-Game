using System.Diagnostics.CodeAnalysis;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly TimeSpan prodDuration;
            public readonly ResRecipe baseResRecipe;
            public readonly ulong prodResPerUnitSurface;
            private readonly ResAmounts buildingCostPerUnitSurface;

            public Factory(string name, ResRecipe baseResRecipe, ulong prodResPerUnitSurface, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, TimeSpan prodDuration, ResAmounts buildingCostPerUnitSurface)
                : base
                (
                    industryType: IndustryType.Manufacturing,
                    name: name,
                    color: Color.Brown,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
                if (baseResRecipe.IsEmpty)
                    throw new ArgumentException();
                this.baseResRecipe = baseResRecipe;
                if (prodResPerUnitSurface == 0)
                    throw new ArgumentException();
                this.prodResPerUnitSurface = prodResPerUnitSurface;
                if (buildingCostPerUnitSurface.IsEmpty())
                    throw new ArgumentException();
                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
            }

            public override Params CreateParams(IIndustryFacingNodeState state)
                => new(state: state, factory: this);

            ResAmounts IFactoryForIndustryWithBuilding.BuildingCost(IIndustryFacingNodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, Building building)
                => new Manufacturing(parameters: CreateParams(state: state), building: building);
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public readonly TimeSpan prodDuration;
            public ResRecipe ResRecipe
                => state.ApproxSurfaceLength * factory.prodResPerUnitSurface * factory.baseResRecipe;

            public override string TooltipText
                => base.TooltipText + $"{nameof(ReqWatts)}: {ReqWatts}\n{nameof(prodDuration)}: {prodDuration}\nSupply: {ResRecipe.results}\nDemand: {ResRecipe.ingredients}\n";

            private readonly Factory factory;

            public Params(IIndustryFacingNodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                prodDuration = factory.prodDuration;
            }
        }

        [Serializable]
        private class Production
        {
            private static Production? Create(ResPile source, ResRecipe recipe, TimeSpan duration)
            {
                if (duration <= TimeSpan.Zero)
                    throw new ArgumentException();
                var resInUse = IngredientsResPile.CreateIfHaveEnough(source: source, recipe: recipe);
                if (resInUse is null)
                    return null;
                return new(resInUse: resInUse, duration: duration);
            }

            public static void Update(ref Production? product, Params parameters, Propor workingPropor)
            {
                if (product is null)
                {
                    product = Create
                    (
                        source: parameters.state.StoredResPile,
                        recipe: parameters.ResRecipe,
                        duration: parameters.prodDuration
                    );
                    return;
                }
                product.prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;
                if (product.prodTimeLeft <= TimeSpan.Zero)
                {
                    var resInUseCopy = product.resInUse;
                    IngredientsResPile.TransformAndTransferAll(ingredients: ref resInUseCopy, destin: parameters.state.StoredResPile);
                    product = null;
                }
            }

            private readonly IngredientsResPile resInUse;
            private TimeSpan prodTimeLeft;
            private readonly TimeSpan duration;

            private Production(IngredientsResPile resInUse, TimeSpan duration)
            {
                this.resInUse = resInUse;
                this.duration = duration;
                prodTimeLeft = duration;
            }

            public Propor DonePropor()
                => C.DonePropor(timeLeft: prodTimeLeft, duration: duration);
        }

        public override bool PeopleWorkOnTop
            => false;

        protected sealed override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;
        private Production? production;

        private Manufacturing(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            production = null;
        }

        protected override BoolWithExplanationIfFalse IsBusy()
            => base.IsBusy() & BoolWithExplanationIfFalse.Create
            (
                value: production is not null,
                explanationIfFalse: "not enough resources\nto start manufacturing\n"
            );

        protected override Manufacturing InternalUpdate(Propor workingPropor)
        {
            Production.Update(product: ref production, parameters: parameters, workingPropor: workingPropor);
            return this;
        }

        public override ResAmounts TargetStoredResAmounts()
            => parameters.ResRecipe.ingredients * parameters.state.MaxBatchDemResStored;

        protected override string GetBusyInfo()
            => $"producing {production!.DonePropor() * 100.0: 0.}%\n";

        protected override UDouble ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy().SwitchExpression
            (
                trueCase: () => parameters.ReqWatts * CurSkillPropor,
                falseCase: () => (UDouble)0
            );
    }
}
