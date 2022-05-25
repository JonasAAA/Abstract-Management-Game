namespace Game1.Industries
{
    public interface IFactoryForIndustryWithBuilding
    {
        // TODO: make all properties upper-case
        public string Name { get; }
        public Color Color { get; }

        public Industry CreateIndustry(NodeState state, Building building);

        public Industry.Params CreateParams(NodeState state);

        public ResAmounts BuildingCost(NodeState state);
    }
}
