using System.Numerics;

namespace Game1.Resources
{
    public interface IOrderedVector<TVector, TScalar> : IAdditionOperators<TVector, TVector, TVector>, IAdditiveIdentity<TVector, TVector>,
        IMultiplyOperators<TVector, TScalar, TVector>, IMultiplicativeIdentity<TVector, TScalar>, IComparisonOperators<TVector, TVector, bool>
        where TVector : IOrderedVector<TVector, TScalar>
        where TScalar : IAdditionOperators<TScalar, TScalar, TScalar>, IAdditiveIdentity<TScalar, TScalar>, IMultiplyOperators<TScalar, TScalar, TScalar>, IMultiplicativeIdentity<TScalar, TScalar>
    { }
}
