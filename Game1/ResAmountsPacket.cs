namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket : IHasMass
    {
        public readonly NodeID destination;
        public readonly ResPile resPile;
        public Mass Mass
            => resPile.Mass;

        public ResAmountsPacket(NodeID destination, MassCounter massCounter)
            : this(destination: destination, resPile: ResPile.CreateEmpty(massCounter: massCounter))
        { }

        private ResAmountsPacket(NodeID destination, ResPile resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }
    }
}
