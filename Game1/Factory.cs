﻿using System;
using System.Collections.Generic;

namespace Game1
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly IntArray demand, supply;
            public readonly TimeSpan prodTime;

            public Params(List<Upgrade> upgrades, IntArray supDemDiff, TimeSpan prodTime)
                : base(upgrades)
            {
                demand = supDemDiff.NegPart;
                supply = supDemDiff.PosPart;
                this.prodTime = prodTime;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Factory(parameters: this, state: state);
        }

        private readonly Params parameters;
        private readonly TimedResQueue production;

        public Factory(Params parameters, NodeState state)
            : base(parameters, state)
        {
            this.parameters = parameters;
            production = new(duration: parameters.prodTime);
        }

        public override void StartProduction()
        {
            base.StartProduction();

            if (production.Empty && state.stored >= parameters.demand)
            {
                state.stored -= parameters.demand;
                production.Enqueue(newResAmounts: parameters.supply);
            }
        }

        public override void FinishProduction()
        {
            base.FinishProduction();

            state.arrived += production.DoneResAmounts();
        }
    }
}
