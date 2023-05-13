using System.Numerics;

namespace Game1.Resources
{
    public interface IResAmounts<T> : IFormOfEnergy<T>, IEquatable<T>, IAdditionOperators<T, T, T>,
        IAdditiveIdentity<T, T>, IMultiplyOperators<T, ulong, T>, IMultiplicativeIdentity<T, ulong>, IMin<T>
        where T : IResAmounts<T>
    {
        public bool IsEmpty { get; }

        public Mass Mass();

        public HeatCapacity HeatCapacity();

        public ulong Area();
    }
}
