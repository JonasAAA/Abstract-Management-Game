using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResAmountsPacket
    {
        public readonly Vector2 destination;
        public ReadOnlyULongArray ResAmounts
            => resAmounts;
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private ReadOnlyULongArray resAmounts;

        public ResAmountsPacket(Vector2 destination)
            : this(destination: destination, resAmounts: new())
        { }

        public ResAmountsPacket(Vector2 destination, ReadOnlyULongArray resAmounts)
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

        public void Add(ReadOnlyULongArray resAmounts)
        {
            this.resAmounts += resAmounts;
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(ResInd resInd, ulong resAmount)
        {
            resAmounts = resAmounts.WithAdd(index: resInd, value: resAmount);
            // TODO: cleanup
            //resAmounts[resInd] += resAmount;
            TotalWeight += CurResConfig.resources[resInd].weight * resAmount;
        }
    }
}
