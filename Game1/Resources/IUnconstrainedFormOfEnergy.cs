namespace Game1.Resources
{
    public interface IUnconstrainedFormOfEnergy<T> : IFormOfEnergy<T>
        where T : IUnconstrainedFormOfEnergy<T>
    {
        public static abstract T CreateFromEnergy(Energy energy);
    }
}
