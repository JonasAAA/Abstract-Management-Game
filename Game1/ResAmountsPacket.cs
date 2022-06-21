namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket : IHasMass
    {
        public readonly NodeID destination;
        public readonly ResPile resPile;
        public ulong Mass
            => resPile.Mass;
        public bool IsEmpty
            => resPile.IsEmpty;

        public ResAmountsPacket(NodeID destination)
            : this(destination: destination, resPile: ResPile.CreateEmpty())
        { }

        private ResAmountsPacket(NodeID destination, ResPile resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }
    }
}
