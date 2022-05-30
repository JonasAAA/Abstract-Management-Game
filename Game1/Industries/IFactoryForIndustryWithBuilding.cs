using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    public interface IFactoryForIndustryWithBuilding
    {
        public string Name { get; }
        public Color Color { get; }

        public sealed Industry CreateIndustry(NodeState state, [DisallowNull] ref Building? building)
        {
            var result = CreateIndustry(state: state, building: building);
            building = null;
            return result;
        }

        protected Industry CreateIndustry(NodeState state, Building building);

        public Industry.Params CreateParams(NodeState state);

        public ResAmounts BuildingCost(NodeState state);
    }
}
