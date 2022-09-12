namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket
    {
        public readonly NodeID destination;
        public readonly ResPile resPile;
        public Mass Mass
            => resPile.Mass;

        public ResAmountsPacket(NodeID destination, LocationCounters locationCounters)
            : this(destination: destination, resPile: ResPile.CreateEmpty(locationCounters: locationCounters))
        { }

        private ResAmountsPacket(NodeID destination, ResPile resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }
    }
}
