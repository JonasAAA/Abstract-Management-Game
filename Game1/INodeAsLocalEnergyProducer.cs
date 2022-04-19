namespace Game1
{
    public interface INodeAsLocalEnergyProducer
    {
        public UDouble LocallyProducedWatts { get; }

        public void SetRemainingLocalWatts(UDouble remainingLocalWatts);
    }
}
