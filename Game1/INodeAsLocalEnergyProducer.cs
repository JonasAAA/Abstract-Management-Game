namespace Game1
{
    public interface INodeAsLocalEnergyProducerAndConsumer
    {
        public void ProduceLocalEnergy<T>(T destin)
            where T : IDestinPile<ElectricalEnergy>;

        public void ConsumeUnusedLocalEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
            where T : ISourcePile<ElectricalEnergy>;
    }
}
