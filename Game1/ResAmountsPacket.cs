using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket
    {
        public readonly NodeID destination;
        public readonly ResPile resPile;
        public ulong TotalMass
            => resPile.TotalMass;
        public bool Empty
            => resPile.IsEmpty;

        public ResAmountsPacket(NodeID destination)
            : this(destination: destination, resPile: ResPile.CreateEmpty())
        { }

        private ResAmountsPacket(NodeID destination, ResPile resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }

        // TODO: delete if unused
        //public void TransferFrom(ResAmountsPacket resAmountsPacket, ResAmounts resAmounts)
        //{
        //    if (resAmountsPacket.destination != destination)
        //        throw new ArgumentException();
        //    TransferFrom(resPile: resAmountsPacket.resPile, resAmounts: resAmounts);
        //}

        //public void TransferFrom(ResPile resPile, ResAmounts resAmounts)
        //    => ResPile.Transfer(source: resPile, destin: this.resPile, resAmounts: resAmounts);

        //public void TRansferFrom(ResPile resPile, ResInd resInd, ulong resAmount)
        //    => ResPile.Transfer(source: resPile, destin: this.resPile, resInd: resInd, resAmount: resAmount);
    }
}
