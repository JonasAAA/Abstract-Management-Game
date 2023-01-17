namespace Game1
{
    /// <summary>
    /// MUST call EnergyManager.AddEnergyConsumer() for each instance
    /// </summary>
    public interface IEnergyConsumer
    {
        public EnergyPriority EnergyPriority { get; }

        /// <summary>
        /// node from which consume energy
        /// </summary>
        public NodeID NodeID { get; }

        public ElectricalEnergy ReqEnergy();

        /// <summary>
        /// Will be called at most once per frame
        /// </summary>
        public void ConsumeEnergyFrom<TSourcePile>(TSourcePile source, ElectricalEnergy electricalEnergy)
            where TSourcePile : ISourcePile<ElectricalEnergy>;
    }
}
