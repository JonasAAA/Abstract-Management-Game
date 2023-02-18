namespace Game1
{
    public interface INodeAsLocalEnergyProducerAndConsumer
    {
        public void ProduceLocalEnergy(EnergyPile<ElectricalEnergy> destin);

        public void ConsumeUnusedLocalEnergyFrom(EnergyPile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy);
    }
}
