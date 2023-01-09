namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyConsumer() for each instance
    /// </summary>
    public interface IEnergyConsumer : IDeletable
    {
        public EnergyPriority EnergyPriority { get; }

        /// <summary>
        /// node from which consume energy
        /// </summary>
        public NodeID NodeID { get; }

        public ElectricalEnergy ReqEnergy();

        public void ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
            where T : IEnergySouce<ElectricalEnergy>;
    }
}
