namespace Game1.Resources
{
    public interface IUnconstrainedEnergy<T> : IFormOfEnergy<T>, IHasToString
        where T : IUnconstrainedEnergy<T>
    {
        public static abstract T CreateFromEnergy(Energy energy);

        public static T CreateFromJoules(ulong valueInJ)
            => T.CreateFromEnergy(energy: Energy.CreateFromJoules(valueInJ: valueInJ));
    }
}
