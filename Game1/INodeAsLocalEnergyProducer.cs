namespace Game1
{
    public interface INodeAsLocalEnergyProducerAndConsumer
    {
        public void ProduceLocalEnergy(Pile<ElectricalEnergy> destin);

        public void ConsumeUnusedLocalEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy);
    }
}
