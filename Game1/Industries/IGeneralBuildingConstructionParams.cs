using Game1.Collections;

namespace Game1.Industries
{
    public interface IGeneralBuildingConstructionParams
    {
        public string Name { get; }
        public EfficientReadOnlyHashSet<IMaterialPurpose> NeededMaterialPurposes { get; }
        //public string TooltipText { get; }

        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

        public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices);
    }
}
