using Game1.ChangingValues;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class Production : ProductiveIndustry
    {
        [Serializable]
        public new abstract class Params : ProductiveIndustry.Params
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly ResAmounts supplyPerUnitSurface;
            public readonly TimeSpan prodDuration;
            
            public Params(IndustryType industryType, string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, ResAmounts supplyPerUnitSurface, TimeSpan prodDuration, string explanation)
                : base
                (
                    industryType: industryType,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkillPerUnitSurface: reqSkillPerUnitSurface,
                    explanation: explanation
                )
            {
                if (MyMathHelper.IsTiny(value: reqWattsPerUnitSurface))
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerUnitSurface = reqWattsPerUnitSurface;
                this.supplyPerUnitSurface = supplyPerUnitSurface;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            protected abstract override Production InternalCreateIndustry(NodeState state);
        }

        private readonly Params parameters;
        private readonly IReadOnlyChangingUDouble reqWatts;
        private readonly IReadOnlyChangingResAmounts supply;
        private TimeSpan prodTimeLeft;

        protected Production(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            reqWatts = parameters.reqWattsPerUnitSurface * state.approxSurfaceLength;
            supply = parameters.supplyPerUnitSurface * state.approxSurfaceLength;
            prodTimeLeft = TimeSpan.MaxValue;
        }

        protected override bool IsBusy()
            => prodTimeLeft < TimeSpan.MaxValue;

        protected abstract bool CanStartProduction();

        protected abstract void StartProduction();

        protected abstract void StopProduction();

        protected override Production InternalUpdate(Propor workingPropor)
        {
            if (IsBusy())
                prodTimeLeft -= workingPropor * CurWorldManager.Elapsed;

            if (!IsBusy() && CanStartProduction())
            {
                prodTimeLeft = parameters.prodDuration;
                StartProduction();
            }

            if (prodTimeLeft <= TimeSpan.Zero)
            {
                state.waitingResAmountsPackets.Add(destination: state.position, resAmounts: supply.Value);
                prodTimeLeft = TimeSpan.MaxValue;
                StopProduction();
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
            if (!CanStartProduction())
                text += "can't start new\n";
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