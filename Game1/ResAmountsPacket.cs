namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket
    {
        public readonly NodeID destination;
        public readonly Pile<ResAmounts> resPile;
        public Mass Mass
            => resPile.Amount.Mass();

        public ResAmountsPacket(NodeID destination, LocationCounters locationCounters)
            : this(destination: destination, resPile: Pile<ResAmounts>.CreateEmpty(locationCounters: locationCounters))
        { }

        private ResAmountsPacket(NodeID destination, Pile<ResAmounts> resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }
    }
}
