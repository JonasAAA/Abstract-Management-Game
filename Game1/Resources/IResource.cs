using static Game1.WorldManager;

namespace Game1.Resources
{
    public interface IResource : IComparable<IResource>, IHasToString
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        /// <summary>
        /// Be careful - useful area is not additive, e.g. if product consists of some components,
        /// to get its useful area can't just add useful areas of the components
        /// </summary>
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }

        int IComparable<IResource>.CompareTo(IResource? other)
        {
            if (other is null)
                return 1;

            return CurResConfig.CompareRes(left: this, right: other);
        }
    }
}
