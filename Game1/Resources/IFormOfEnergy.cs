namespace Game1.Resources
{
    public interface IFormOfEnergy<T> : ICountable<T>
        where T : IFormOfEnergy<T>
    {
        public Energy Energy { get; }
    }
}
