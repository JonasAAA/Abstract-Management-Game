using Game1.Collections;

namespace Game1.Industries
{
    public interface IBuildingConcreteParams : IIncompleteBuildingImage
    {
        ///// <summary>
        ///// The total area of products and materials used is the same for everything, this is just used to calculate the relative amounts of products needes
        ///// </summary>
        //public GeneralProdAndMatAmounts buildingCostPropors { get; }

        public SomeResAmounts<IResource> BuildingCost { get; }

        public IIndustry CreateIndustry(ResPile buildingResPile);
    }
}
