namespace Game1.Resources
{
    public interface IResource
    {
        public ResInd ResInd { get; }
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public Propor Reflectance { get; }
        public Propor Emissivity { get; }
        public ResAmounts BasicIngredients { get; }
    }
}
