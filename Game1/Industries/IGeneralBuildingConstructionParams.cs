using Game1.Collections;

namespace Game1.Industries
{
    public interface IGeneralBuildingConstructionParams
    {
        public string Name { get; }
        public GeneralProdAndMatAmounts BuildingCostPropors { get; }

        public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices);
    }
}
