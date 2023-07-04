namespace Game1.Industries
{
    public interface IConcreteBuildingConstructionParams : IIncompleteBuildingImage
    {
        /// <summary>
        /// Must be constant
        /// </summary>
        public AllResAmounts BuildingCost { get; }

        public IIndustry CreateIndustry(ResPile buildingResPile);
    }
}
