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
        /// node position from which consume energy
        /// </summary>
        public MyVector2 NodePos { get; }

        public UDouble ReqWatts();

        public void ConsumeEnergy(Propor energyPropor);
    }
}
