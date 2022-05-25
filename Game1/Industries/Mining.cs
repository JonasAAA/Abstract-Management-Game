using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly UDouble minedResPerUnitSurfacePerSec;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, UDouble minedResPerUnitSurfacePerSec)
                : base
                (
                    industryType: IndustryType.Mining,
                    name: name,
                    color: Color.Pink,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.minedResPerUnitSurfacePerSec = minedResPerUnitSurfacePerSec;
            }

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => Name;

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);

            Industry IBuildableFactory.CreateIndustry(NodeState state)
                => new Mining(parameters: CreateParams(state: state));
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public UDouble MinedResPerSec
                => state.ApproxSurfaceLength * factory.minedResPerUnitSurfacePerSec;

            public override string TooltipText
                => base.TooltipText + $"{nameof(ReqWatts)}: {ReqWatts}\n{nameof(MinedResPerSec)}: {MinedResPerSec}\n";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        [Serializable]
        private readonly record struct FutureShapeOutlineParams(Params Parameters) : Ring.IParamsWithOuterRadius
        {
            public MyVector2 Center
                => Parameters.state.position;

            public UDouble OuterRadius
                => Parameters.state.Radius + 1;
        }

        public override bool PeopleWorkOnTop
            => true;

        protected override UDouble Height
            => 0;

        private readonly Params parameters;
        /// <summary>
        /// Since each frame a non-integer amount will be mined, and resources can only be moved in integer amounts,
        /// this represents the amount that is mined, but not counted yet. Must be between 0 and 1.
        /// </summary>
        private UDouble silentlyMinedBits;
        private UDouble minedResPerSec;
        private readonly OuterRing futureShapeOutline;

        private Mining(Params parameters)
            : base(parameters: parameters, building: null)
        {
            this.parameters = parameters;
            silentlyMinedBits = 0;
            minedResPerSec = 0;
            futureShapeOutline = new(parameters: new FutureShapeOutlineParams(Parameters: parameters));
        }

        protected override bool IsBusy()
            => parameters.state.MaxAvailableResAmount > 0;

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override Mining InternalUpdate(Propor workingPropor)
        {
            UDouble targetMinedRes = workingPropor * parameters.MinedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds,
                resToMine = targetMinedRes + silentlyMinedBits;
            ulong minedRes = (ulong)resToMine;
            silentlyMinedBits = (UDouble)(resToMine - minedRes);
            Debug.Assert(0 <= silentlyMinedBits && silentlyMinedBits <= 1);

            ulong maxMinedRes = parameters.state.MaxAvailableResAmount;
            if (minedRes > maxMinedRes)
            {
                minedRes = maxMinedRes;
                silentlyMinedBits = 0;
            }

            minedResPerSec = MyMathHelper.Min(targetMinedRes, maxMinedRes) / (UDouble)CurWorldManager.Elapsed.TotalSeconds;
            parameters.state.MineTo(destin: parameters.state.storedResPile, resAmount: minedRes);

            return this;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"Mining {minedResPerSec:0.##} {parameters.state.consistsOfResInd} per second\n";
            else
                text += "The planet is fully mined out, can't mine anymore\n";
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

        public override void DrawAfterPlanet()
        {
            base.DrawAfterPlanet();

            futureShapeOutline.Draw(color: Color.Black);
        }
    }
}
