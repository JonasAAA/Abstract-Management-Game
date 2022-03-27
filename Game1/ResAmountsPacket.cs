using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResAmountsPacket
    {
        public readonly Vector2 destination;
        public ConstULongArray ResAmounts
            => resAmounts;
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private ULongArray resAmounts;

        public ResAmountsPacket(Vector2 destination)
            : this(destination: destination, resAmounts: new())
        { }

        public ResAmountsPacket(Vector2 destination, ConstULongArray resAmounts)
        {
            this.destination = destination;
            this.resAmounts = resAmounts.ToULongArray();
            TotalWeight = resAmounts.TotalWeight();
        }

        public void Add(ResAmountsPacket resAmountsPacket)
        {
            if (resAmountsPacket.destination != destination)
                throw new ArgumentException();
            resAmounts += resAmountsPacket.resAmounts;
            TotalWeight += resAmountsPacket.TotalWeight;
        }

        public void Add(ConstULongArray resAmounts)
        {
            this.resAmounts += resAmounts;
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(int resInd, ulong resAmount)
        {
            resAmounts[resInd] += resAmount;
            TotalWeight += CurResConfig.resources[resInd].weight * resAmount;
        }
    }
}
