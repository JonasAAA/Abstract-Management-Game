namespace Game1.Resources
{
    public interface IFormOfEnergy<T> : ICountable<T>
        where T : IFormOfEnergy<T>
    {
        public abstract static explicit operator Energy(T formOfEnergy);
    }
}
