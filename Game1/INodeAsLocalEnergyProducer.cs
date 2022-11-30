namespace Game1
{
    public interface INodeAsLocalEnergyProducerAndConsumer
    {
        public void ProduceLocalEnergy<T>(T destin)
            where T : IEnergyDestin<ElectricalEnergy>;

        public void SetUsedLocalEnergy(Energy usedLocalEnergy);

        public void ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
            where T : IEnergySouce<ElectricalEnergy>;
    }
}
