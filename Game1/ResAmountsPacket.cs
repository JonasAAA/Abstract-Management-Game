using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResAmountsPacket
    {
        public readonly NodeId destination;
        public ResAmounts ResAmounts
            => resAmounts;
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private ResAmounts resAmounts;

        public ResAmountsPacket(NodeId destination)
            : this(destination: destination, resAmounts: new())
        { }

        public ResAmountsPacket(NodeId destination, ResAmounts resAmounts)
        {
            this.destination = destination;
            this.resAmounts = resAmounts;
            TotalWeight = resAmounts.TotalWeight();
        }

        public void Add(ResAmountsPacket resAmountsPacket)
        {
            if (resAmountsPacket.destination != destination)
                throw new ArgumentException();
            resAmounts += resAmountsPacket.resAmounts;
            TotalWeight += resAmountsPacket.TotalWeight;
        }

        public void Add(ResAmounts resAmounts)
        {
            this.resAmounts += resAmounts;
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(ResInd resInd, ulong resAmount)
        {
            resAmounts = resAmounts.WithAdd(index: resInd, value: resAmount);
            TotalWeight += CurResConfig.resources[resInd].mass * resAmount;
        }
    }
}
