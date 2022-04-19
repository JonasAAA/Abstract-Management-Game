using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class Production : ProductiveIndustry
    {
        [Serializable]
        public new abstract class Factory : ProductiveIndustry.Factory
        {
            public readonly UDouble reqWattsPerUnitSurface;
            public readonly TimeSpan prodDuration;

            public Factory(IndustryType industryType, string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, UDouble reqWattsPerUnitSurface, TimeSpan prodDuration, string explanation)
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
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            public abstract override Production CreateIndustry(NodeState state);
        }

        [Serializable]
        public new abstract class Params : ProductiveIndustry.Params
        {
            public UDouble ReqWatts
                => state.ApproxSurfaceLength * factory.reqWattsPerUnitSurface;
            public ResAmounts Supply
                => state.ApproxSurfaceLength * SupplyPerUnitSurface;
            public readonly TimeSpan prodDuration;

            protected abstract ResAmounts SupplyPerUnitSurface { get; }

            private readonly Factory factory;

            public Params(NodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                prodDuration = factory.prodDuration;
            }
        }

        private readonly Params parameters;
        private TimeSpan prodTimeLeft;

        protected Production(Params parameters)
            : base(parameters: parameters)
        {
            this.parameters = parameters;
            // TODO: delete
            //reqWatts = parameters.reqWattsPerUnitSurface * state.approxSurfaceLength;
            //supply = parameters.supplyPerUnitSurface * state.approxSurfaceLength;
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
                parameters.state.waitingResAmountsPackets.Add(destination: parameters.state.nodeId, resAmounts: parameters.Supply);
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
                true => parameters.ReqWatts * CurSkillPropor,
                false => 0
            };
    }
}