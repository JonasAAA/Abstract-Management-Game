using Game1.Delegates;
using Game1.UI;

namespace Game1.Industries
{
    public interface IGeneralBuildingConstructionParams
    {
        public IFunction<IHUDElement> NameVisual { get; }

        public BuildingCostPropors BuildingCostPropors { get; }

        /// <summary>
        /// Return null if no production choice is needed
        /// </summary>
        public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter);

        public sealed IConcreteBuildingConstructionParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
        {
            if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.Choices.Keys))
                throw new ArgumentException();
            return CreateConcreteImpl(nodeState: nodeState, neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices, productionChoice: productionChoice);
        }

        public IConcreteBuildingConstructionParams CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice);
    }
}
