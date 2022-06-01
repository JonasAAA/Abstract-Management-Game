using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    public interface IFactoryForIndustryWithBuilding
    {
        public string Name { get; }
        public Color Color { get; }

        public sealed Industry CreateIndustry(IIndustryFacingNodeState state, [DisallowNull] ref Building? building)
        {
            var result = CreateIndustry(state: state, building: building);
            building = null;
            return result;
        }

        protected Industry CreateIndustry(IIndustryFacingNodeState state, Building building);

        public Industry.Params CreateParams(IIndustryFacingNodeState state);

        public ResAmounts BuildingCost(IIndustryFacingNodeState state);
    }
}
