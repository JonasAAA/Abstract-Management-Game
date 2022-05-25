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

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            ResAmounts IFactoryForIndustryWithBuilding.BuildingCost(NodeState state)
                => state.ApproxSurfaceLength * buildingCostPerUnitSurface;

            Industry IFactoryForIndustryWithBuilding.CreateIndustry(NodeState state, Building building)
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

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                prodDuration = factory.prodDuration;
            }
        }

        public override bool PeopleWorkOnTop
            => false;

        protected sealed override UDouble Height
            => CurWorldConfig.defaultIndustryHeight;

        private readonly Params parameters;
        private TimeSpan prodTimeLeft;
        private readonly ResPile resInUse;
        private ResRecipe curResRecipe;

        private Manufacturing(Params parameters, Building building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            resInUse = ResPile.CreateEmpty();
            curResRecipe = parameters.ResRecipe;
        }

        protected override bool IsBusy()
            => prodTimeLeft < TimeSpan.MaxValue;

        protected override Manufacturing InternalUpdate(Propor workingPropor)
        {
            if (IsBusy())
                prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (!IsBusy())
            {
                curResRecipe = parameters.ResRecipe;
                if (CanStartProduction())
                {
                    prodTimeLeft = parameters.prodDuration;
                    ResPile.Transfer(source: parameters.state.storedResPile, destin: resInUse, resAmounts: parameters.ResRecipe.ingredients);
                }
            }

            if (prodTimeLeft <= TimeSpan.Zero)
            {
                resInUse.TransformAll(resRecipe: curResRecipe);
                ResPile.TransferAll(source: resInUse, destin: parameters.state.storedResPile);
                prodTimeLeft = TimeSpan.MaxValue;
            }

            return this;
        }

        private bool CanStartProduction()
            => parameters.state.storedResPile.ResAmounts >= curResRecipe.ingredients;

        public override ResAmounts TargetStoredResAmounts()
            => curResRecipe.ingredients * parameters.state.maxBatchDemResStored;

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"producing {C.DonePropor(timeLeft: prodTimeLeft, duration: parameters.prodDuration) * 100.0: 0.}%\n";
            else
                text += "idle\n";
            if (!CanStartProduction())
                text += "can't start new\n";
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
    }
}
