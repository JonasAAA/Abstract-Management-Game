namespace Game1.Industries
{
    public interface IConstructedIndustryGeneralParams
    {
        public string Name { get; }
        public Color Color { get; }

        /// <summary>
        /// The total area of products and materials used is the same for everything, this is just used to calculate the relative amounts of products needes
        /// </summary>
        public GeneralProdAndMatAmounts BuildingCostPropors { get; }

        public IIndustry CreateIndustry(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices, ResPile buildingResPile);

        //public Industry CreateIndustry(IIndustryFacingNodeState state, Building building);

        //public Industry.GeneralParams CreateParams(IIndustryFacingNodeState state);

        //public SomeResAmounts<IResource> BuildingCost(IIndustryFacingNodeState state);
    }
}
