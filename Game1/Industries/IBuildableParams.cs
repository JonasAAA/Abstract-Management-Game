namespace Game1.Industries
{
    /// <summary>
    /// Signifies that these parameters can be used straight-up, no need to be built first
    /// </summary>
    public interface IBuildableParams
    {
        public string Explanation { get; }

        public string ButtonName { get; }

        public bool CanCreateWith(NodeState state);

        public Industry CreateIndustry(NodeState state);
    }
}
