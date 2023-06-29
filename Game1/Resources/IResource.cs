using Game1.Collections;

namespace Game1.Resources
{
    public interface IResource
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        /// <summary>
        /// Be careful - useful area is not additive, e.g. if product consists of some components,
        /// to get its useful area can't just add useful areas of the components
        /// </summary>
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }
    }
}
