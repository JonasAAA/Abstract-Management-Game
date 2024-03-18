using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    public interface IResource : IComparable<IResource>, IHasToString
    {
        // This is not Icon type because want to allow ConfigurableIcon as well and don't want to use inheritance
        public IImage Icon { get; }
        public IImage SmallIcon { get; }
        public Mass Mass { get; }
        /// <summary>
        /// Heat capacity per area
        /// </summary>
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }

        int IComparable<IResource>.CompareTo(IResource? other)
        {
            if (other is null)
                return 1;

            return CurResConfig.CompareRes(left: this, right: other);
        }
    }
}
