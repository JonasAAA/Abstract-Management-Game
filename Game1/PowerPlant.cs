using System;
using System.Collections.Generic;

namespace Game1
{
    public class PowerPlant : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly ulong prodWattsPerSec;

            public Params(string name, List<Upgrade> upgrades, ulong prodWattsPerSec)
                : base(name: name, upgrades: upgrades)
            {
                if (prodWattsPerSec <= 0)
                    throw new ArgumentOutOfRangeException();
                this.prodWattsPerSec = prodWattsPerSec;
            }

            public override Industry MakeIndustry(NodeState state)
                => new PowerPlant(parameters: this, state: state);
        }

        private readonly Params parameters;

        public PowerPlant(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
        }

        public override ulong ProdWattsPerSec()
            => base.ProdWattsPerSec() + parameters.prodWattsPerSec;
    }
}
