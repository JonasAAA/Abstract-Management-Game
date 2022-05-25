using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class PlanetEnlargement : ProductiveIndustry
    {
        [Serializable]
        public new sealed class Factory : ProductiveIndustry.Factory, IBuildableFactory
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly UDouble addedResPerUnitSurfacePerSec;

            public Factory(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, UDouble addedResPerUnitSurfacePerSec)
                : base
                (
                    industryType: IndustryType.PlanetEnlargement,
                    name: name,
                    color: Color.Pink,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.addedResPerUnitSurfacePerSec = addedResPerUnitSurfacePerSec;
            }

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => Name;

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);

            Industry IBuildableFactory.CreateIndustry(NodeState state)
                => new PlanetEnlargement(parameters: CreateParams(state: state));
        }

        [Serializable]
        public new sealed class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public UDouble AddedResPerSec
                => state.ApproxSurfaceLength * factory.addedResPerUnitSurfacePerSec;

            public override string TooltipText
                => base.TooltipText + $"{nameof(ReqWatts)}: {ReqWatts}\n{nameof(AddedResPerSec)}: {AddedResPerSec}\n";

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;
            }
        }

        [Serializable]
        private readonly record struct FutureShapeOutlineParams(Params Parameters) : Ring.IParamsWithInnerRadius
        {
            public MyVector2 Center
                => Parameters.state.position;

            public UDouble InnerRadius
                => Parameters.state.Radius;
        }

        public override bool PeopleWorkOnTop
            => true;

        protected override UDouble Height
            => 0;

        private readonly Params parameters;
        /// <summary>
        /// Since each frame a non-integer amount will be added, and resources can only be moved in integer amounts,
        /// this represents the amount that is added, but not counted yet. Must be between 0 and 1.
        /// </summary>
        private UDouble silentlyAddedBits;
        private UDouble curAddedResPerSec;
        private readonly InnerRing futureShapeOutline;

        private PlanetEnlargement(Params parameters)
            : base(parameters: parameters, building: null)
        {
            this.parameters = parameters;
            silentlyAddedBits = 0;
            curAddedResPerSec = 0;
            futureShapeOutline = new(parameters: new FutureShapeOutlineParams(Parameters: parameters));
        }

        protected override bool IsBusy()
            => true;

        public override ResAmounts TargetStoredResAmounts()
            => new()
            {
                [parameters.state.consistsOfResInd] = (ulong)(parameters.AddedResPerSec * 60)
            };

        protected override PlanetEnlargement InternalUpdate(Propor workingPropor)
        {
            UDouble targetAddedRes = workingPropor * parameters.AddedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds,
                resToAdd = targetAddedRes + silentlyAddedBits;
            ulong addedRes = (ulong)resToAdd;
            silentlyAddedBits = (UDouble)(resToAdd - addedRes);
            Debug.Assert(0 <= silentlyAddedBits && silentlyAddedBits <= 1);

            ulong maxAddedRes = parameters.state.storedResPile[parameters.state.consistsOfResInd];
            if (addedRes > maxAddedRes)
            {
                addedRes = maxAddedRes;
                silentlyAddedBits = 0;
            }

            curAddedResPerSec = MyMathHelper.Min(targetAddedRes, maxAddedRes) / (UDouble)CurWorldManager.Elapsed.TotalSeconds;
            parameters.state.EnlargeFrom(source: parameters.state.storedResPile, resAmount: addedRes);

            return this;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            text += $"Adding {curAddedResPerSec:0.##} {parameters.state.consistsOfResInd} per second\n";
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
            base.DrawBeforePlanet(otherColor, otherColorPropor);

            futureShapeOutline.Draw(baseColor: parameters.state.consistsOfRes.color, otherColor: otherColor, otherColorPropor: otherColorPropor);
        }
    }
}
