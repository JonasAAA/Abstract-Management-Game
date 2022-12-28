namespace Game1
{
    public interface INodeAsLocalEnergyProducerAndConsumer
    {
        public void ProduceLocalEnergy<T>(T destin)
            where T : IEnergyDestin<ElectricalEnergy>;

        public void ConsumeUnusedLocalEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
            where T : IEnergySouce<ElectricalEnergy>;
    }
}
