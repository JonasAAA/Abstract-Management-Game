using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class PlanetEnlargement : ProductiveIndustry
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
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.addedResPerUnitSurfacePerSec = addedResPerUnitSurfacePerSec;
            }

            public override PlanetEnlargement CreateIndustry(NodeState state)
                => new(parameters: CreateParams(state: state));

            public override Params CreateParams(NodeState state)
                => new(state: state, factory: this);

            string IBuildableFactory.ButtonName
                => name;

            ITooltip IBuildableFactory.CreateTooltip(NodeState state)
                => Tooltip(state: state);
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

        private readonly Params parameters;
        /// <summary>
        /// Since each frame a non-integer amount will be added, and resources can only be moved in integer amounts,
        /// this represents the amount that is added, but not counted yet. Must be between 0 and 1.
        /// </summary>
        private UDouble silentlyAddedBits;
        private UDouble curAddedResPerSec;

        public PlanetEnlargement(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            silentlyAddedBits = 0;
            curAddedResPerSec = 0;
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
            UDouble resToAdd = workingPropor * parameters.AddedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds + silentlyAddedBits;
            ulong addedRes = (ulong)resToAdd;
            silentlyAddedBits = (UDouble)(resToAdd - addedRes);
            Debug.Assert(0 <= silentlyAddedBits && silentlyAddedBits <= 1);

            ulong maxAddedRes = parameters.state.storedRes[parameters.state.consistsOfResInd];
            if (addedRes > maxAddedRes)
            {
                addedRes = maxAddedRes;
                silentlyAddedBits = 0;
            }

            curAddedResPerSec = addedRes / (UDouble)CurWorldManager.Elapsed.TotalSeconds;

            parameters.state.storedRes -= new ResAmounts()
            {
                [parameters.state.consistsOfResInd] = addedRes
            };
            parameters.state.AddRes(resAmount: addedRes);

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
    }
}
