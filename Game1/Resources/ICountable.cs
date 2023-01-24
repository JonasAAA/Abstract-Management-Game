using System.Numerics;

namespace Game1.Resources
{
    public interface ICountable<T> : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparisonOperators<T, T, bool>
        where T : ICountable<T>
    { }
}
