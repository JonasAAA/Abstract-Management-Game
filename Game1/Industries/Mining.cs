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
                    industryType: IndustryType.Factory,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
                )
            {
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.minedResPerUnitSurfacePerSec = minedResPerUnitSurfacePerSec;
            }

            public override Mining CreateIndustry(NodeState state)
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

        private readonly Params parameters;
        /// <summary>
        /// Since each frame a non-integer amount will be mined, and resources can only be moved in integer amounts,
        /// this represents the amount that is mined, but not counted yet. Must be between 0 and 1.
        /// </summary>
        private UDouble silentlyMinedBits;
        private UDouble curMinedResPerSec;

        public Mining(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            silentlyMinedBits = 0;
            curMinedResPerSec = 0;
        }

        protected override bool IsBusy()
            => parameters.state.MaxAvailableResAmount > 0;

        public override ResAmounts TargetStoredResAmounts()
            => new();

        protected override Mining InternalUpdate(Propor workingPropor)
        {
            curMinedResPerSec = workingPropor * parameters.MinedResPerSec;
            UDouble resToMine = curMinedResPerSec * (UDouble)CurWorldManager.Elapsed.TotalSeconds + silentlyMinedBits;
            ulong minedRes = (ulong)resToMine;
            silentlyMinedBits = (UDouble)(resToMine - minedRes);
            Debug.Assert(0 <= silentlyMinedBits && silentlyMinedBits <= 1);

            if (minedRes > parameters.state.MaxAvailableResAmount)
            {
                minedRes = parameters.state.MaxAvailableResAmount;
                silentlyMinedBits = 0;
            }

            parameters.state.waitingResAmountsPackets.Add(destination: parameters.state.nodeID, resInd: parameters.state.consistsOfResInd, resAmount: minedRes);
            parameters.state.Remove(resAmount: minedRes);

            return this;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"Mining {curMinedResPerSec:0.##} {parameters.state.consistsOfResInd} per second\n";
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
    }
}
