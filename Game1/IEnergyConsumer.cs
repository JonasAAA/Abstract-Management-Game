namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyConsumer() for each instance
    /// </summary>
    public interface IEnergyConsumer
    {
        public EnergyPriority EnergyPriority { get; }

        /// <summary>
        /// Node from which consume energy
        /// This is here so that energy consumer could travel between nodes (eg person)
        /// </summary>
        public NodeID NodeID { get; }

        public ElectricalEnergy ReqEnergy();

        /// <summary>
        /// Will be called at most once per frame
        /// </summary>
        public void ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy);
    }
}
