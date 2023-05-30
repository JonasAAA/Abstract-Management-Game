using Game1.Collections;

namespace Game1.Resources
{
    public interface IResource
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        //public Area Area { get; }
        public RawMaterialsMix RawMatComposition { get; }
    }
}
