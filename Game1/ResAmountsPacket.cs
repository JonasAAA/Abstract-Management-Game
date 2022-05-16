using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket
    {
        public readonly NodeID destination;
        public ResAmounts ResAmounts
            => resAmounts;
        public ulong TotalMass { get; private set; }
        public bool Empty
            => TotalMass is 0;

        private ResAmounts resAmounts;

        public ResAmountsPacket(NodeID destination)
            : this(destination: destination, resAmounts: new())
        { }

        public ResAmountsPacket(NodeID destination, ResAmounts resAmounts)
        {
            this.destination = destination;
            this.resAmounts = resAmounts;
            TotalMass = resAmounts.TotalMass();
        }

        public void Add(ResAmountsPacket resAmountsPacket)
        {
            if (resAmountsPacket.destination != destination)
                throw new ArgumentException();
            resAmounts += resAmountsPacket.resAmounts;
            TotalMass += resAmountsPacket.TotalMass;
        }

        public void Add(ResAmounts resAmounts)
        {
            this.resAmounts += resAmounts;
            TotalMass += resAmounts.TotalMass();
        }

        public void Add(ResInd resInd, ulong resAmount)
        {
            resAmounts = resAmounts.WithAdd(index: resInd, value: resAmount);
            TotalMass += CurResConfig.resources[resInd].Mass * resAmount;
        }
    }
}
