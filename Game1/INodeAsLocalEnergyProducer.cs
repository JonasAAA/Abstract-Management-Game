namespace Game1
{
    public interface INodeAsLocalEnergyProducer
    {
        public UDouble LocallyProducedWatts { get; }

        public void SetUsedLocalWatts(UDouble remainingLocalWatts);
    }
}
