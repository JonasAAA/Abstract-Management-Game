namespace Game1.Industries
{
    [Serializable]
    public sealed class IndustryConfig
    {
        public readonly ReadOnlyCollection<IBuildableFactory> constrBuildingParams;
        //public readonly House.Factory basicHouseFactory;
        //public readonly PowerPlant.Factory basicPowerPlantFactory;

        public IndustryConfig()
        {
            throw new NotImplementedException();
        }
    }
}
