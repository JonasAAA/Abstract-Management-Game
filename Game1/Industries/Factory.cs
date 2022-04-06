using Game1.ChangingValues;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public class Factory : ProductiveIndustry
    {
        [Serializable]
        public new class Params : ProductiveIndustry.Params
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly ResAmounts supplyPerUnitSurface, demandPerUnitSurface;
            public readonly TimeSpan prodDuration;
            
            public Params(string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, ResAmounts demandPerUnitSurface, TimeSpan prodDuration)
                : base
                (
                    industryType: IndustryType.Production,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: $"{nameof(reqSkillPerUnitSurface)} {reqSkillPerUnitSurface}\n{nameof(reqWattsPerUnitSurface)} {reqWattsPerUnitSurface}\n{nameof(supplyPerUnitSurface)} {supplyPerUnitSurface}\n{nameof(demandPerUnitSurface)} {demandPerUnitSurface}"
                )
            {
                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.supplyPerUnitSurface = supplyPerUnitSurface;
                this.demandPerUnitSurface = demandPerUnitSurface;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            public override Factory MakeIndustry(NodeState state)
                => new(state: state, parameters: this);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingUDouble reqWatts;
        private readonly IReadOnlyChangingResAmounts supply, demand;
        private TimeSpan prodTimeLeft;

        private Factory(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            reqWatts = parameters.reqWattsPerUnitSurface * state.approxSurfaceLength;
            supply = parameters.supplyPerUnitSurface * state.approxSurfaceLength;
            demand = parameters.demandPerUnitSurface * state.approxSurfaceLength;
            prodTimeLeft = TimeSpan.MaxValue;
        }

        public override ResAmounts TargetStoredResAmounts()
        {
            if (CanStartProduction)
                return demand.Value * state.maxBatchDemResStored;
            return new();
        }

        protected override bool IsBusy()
            => prodTimeLeft < TimeSpan.MaxValue;

        protected override Factory InternalUpdate(Propor workingPropor)
        {
            if (IsBusy())
                prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (CanStartProduction && !IsBusy() && state.storedRes >= demand.Value)
            {
                state.storedRes -= demand.Value;
                prodTimeLeft = parameters.prodDuration;
            }

            if (prodTimeLeft <= TimeSpan.Zero)
            {
                state.waitingResAmountsPackets.Add(destination: state.position, resAmounts: supply.Value);
                prodTimeLeft = TimeSpan.MaxValue;
            }

            return this;
        }

        public override string GetInfo()
        {
            string text = base.GetInfo() + $"{parameters.name}\n";
            if (IsBusy())
                text += $"producing {C.DonePropor(timeLeft: prodTimeLeft, duration: parameters.prodDuration) * 100.0: 0.}%\n";
            else
                text += "idle\n";
            if (!CanStartProduction)
                text += "will not start new\n";
            return text;
        }

        public override UDouble ReqWatts()
            // this is correct as if more important people get full energy, this works
            // and if they don't, then the industry will get 0 energy anyway
            => IsBusy() switch
            {
                true => reqWatts.Value * CurSkillPropor,
                false => 0
            };
    }
}