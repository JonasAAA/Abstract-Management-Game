namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyConsumer() for each instance
    /// </summary>
    public interface IEnergyConsumer : IDeletable
    {
        /// <summary>
        /// the lower, the more important
        /// </summary>
        public EnergyPriority EnergyPriority { get; }

        /// <summary>
        /// node from which consume energy
        /// </summary>
        // TODO: rename to Node
        public NodeId NodeId { get; }

        public UDouble ReqWatts();

        public void ConsumeEnergy(Propor energyPropor);
    }
}
