using System.Numerics;

namespace Game1
{
    public interface ICountable<T> : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IEqualityOperators<T, T, bool>
        where T : ICountable<T>
    { }
}
