using Game1.Collections;

namespace Game1.Industries
{
    public interface IBuildingGeneralParams
    {
        public string Name { get; }

        /// <summary>
        /// Keys contain ALL material purposes, not just used ones
        /// </summary>
        public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

        public Result<IBuildingConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices);
    }
}
