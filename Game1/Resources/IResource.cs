namespace Game1.Resources
{
    public interface IResource
    {
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }
    }
}
