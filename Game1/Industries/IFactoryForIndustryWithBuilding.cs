using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    public interface IFactoryForIndustryWithBuilding
    {
        public string Name { get; }
        public Color Color { get; }

        public Industry CreateIndustry(IIndustryFacingNodeState state, Building building);

        public Industry.Params CreateParams(IIndustryFacingNodeState state);

        public ResAmounts BuildingCost(IIndustryFacingNodeState state);
    }
}
