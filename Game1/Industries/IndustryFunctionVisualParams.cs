using Game1.UI;

namespace Game1.Industries
{
    public readonly record struct IndustryFunctionVisualParams(IEnumerable<ConfigurableIcon> InputIcons, IEnumerable<ConfigurableIcon> OutputIcons);
}
