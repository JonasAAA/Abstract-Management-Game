namespace Game1.Industries
{
    public interface IGeneralBuildingConstructionParams
    {
        public string Name { get; }
        public BuildingCostPropors BuildingCostPropors { get; }

        public sealed IConcreteBuildingConstructionParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices)
        {
            if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.choices.Keys))
                throw new ArgumentException();
            return CreateConcreteImpl(nodeState: nodeState, neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices);
        }

        public IConcreteBuildingConstructionParams CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices);
    }
}
