using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game1
{
    public class Industry
    {
        public class Params
        {
            public readonly ReadOnlyCollection<Upgrade> upgrades;

            public Params(List<Upgrade> upgrades)
            {
                this.upgrades = new(upgrades);
            }

            public virtual Industry MakeIndustry(NodeState state)
                => new(parameters: this, state: state);
        }

        public class Upgrade
        {
            public readonly Params parameters;
            public readonly TimeSpan minTime;
            public readonly ConstUIntArray cost;

            public Upgrade(Params parameters, TimeSpan minTime, ConstUIntArray cost)
            {
                this.parameters = parameters;
                this.minTime = minTime;
                this.cost = cost;
            }
        }

        public static readonly Params emptyParams;

        static Industry()
        {
            emptyParams = new Params
            (
                upgrades: new() { }
            );
        }

        public readonly NodeState state;
        private readonly Params parameters;

        public Industry(Params parameters, NodeState state)
        {
            this.parameters = parameters;
            this.state = state;
        }

        public virtual void StartProduction()
        { }

        public virtual void FinishProduction()
        { }
    }
}
