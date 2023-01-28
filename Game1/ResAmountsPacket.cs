namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacket
    {
        public readonly NodeID destination;
        public readonly ResPile resPile;
        public Mass Mass
            => resPile.Amount.Mass();

        public ResAmountsPacket(NodeID destination, ThermalBody thermalBody)
            : this(destination: destination, resPile: ResPile.CreateEmpty(thermalBody: thermalBody))
        { }

        private ResAmountsPacket(NodeID destination, ResPile resPile)
        {
            this.destination = destination;
            this.resPile = resPile;
        }
    }
}
