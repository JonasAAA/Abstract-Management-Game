using Game1.UI;

namespace Game1.Industries
{
    /// <summary>
    /// Signifies that these parameters can be used straight-up, no need to be built first
    /// </summary>
    public interface IBuildableFactory
    {
        public string ButtonName { get; }

        public Industry CreateIndustry(NodeState state);

        public ITooltip CreateTooltip(NodeState state);
    }
}
