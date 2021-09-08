using System;

namespace Game1
{
    public class ResAmountsPacket
    {
        public readonly Position destination;
        public ConstULongArray ResAmounts
            => resAmounts;
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private ULongArray resAmounts;

        public ResAmountsPacket(Position destination)
            : this(destination: destination, resAmounts: new())
        { }

        public ResAmountsPacket(Position destination, ConstULongArray resAmounts)
        {
            if (destination is null)
                throw new ArgumentNullException();
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
            TotalWeight += Resource.all[resInd].weight * resAmount;
        }
    }
}
