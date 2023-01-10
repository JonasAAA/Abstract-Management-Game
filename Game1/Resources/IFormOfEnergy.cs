using System.Numerics;

namespace Game1.Resources
{
    public interface IFormOfEnergy<T> : ICountable<T>
        where T : IFormOfEnergy<T>
    {
        public bool IsZero { get; }

        public abstract static explicit operator Energy(T formOfEnergy);
    }
}
