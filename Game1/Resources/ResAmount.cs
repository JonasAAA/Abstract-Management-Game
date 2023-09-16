using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmount<TRes> : IMultiplyOperators<ResAmount<TRes>, UInt96, ResAmount<TRes>>, IMultiplicativeIdentity<ResAmount<TRes>, UInt96>
        where TRes : class, IResource
    {
        static UInt96 IMultiplicativeIdentity<ResAmount<TRes>, UInt96>.MultiplicativeIdentity
            => 1;

        public readonly TRes res;
        public readonly UInt96 amount;

        public ResAmount(TRes res, UInt96 amount)
        {
            this.res = res;
            this.amount = amount;
        }

        public void Deconstruct(out TRes res, out UInt96 amount)
        {
            res = this.res;
            amount = this.amount;
        }

        public static ResAmount<TRes> operator *(UInt96 left, ResAmount<TRes> right)
            => new(res: right.res, amount: left * right.amount);

        public static ResAmount<TRes> operator *(ResAmount<TRes> left, UInt96 right)
            => right * left;
    }
}
