using System.Numerics;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmount<TRes>(TRes res, ulong amount) : IMultiplyOperators<ResAmount<TRes>, ulong, ResAmount<TRes>>, IMultiplicativeIdentity<ResAmount<TRes>, ulong>
        where TRes : class, IResource
    {
        static ulong IMultiplicativeIdentity<ResAmount<TRes>, ulong>.MultiplicativeIdentity
            => 1;

        public readonly TRes res = res;
        public readonly ulong amount = amount;

        public void Deconstruct(out TRes res, out ulong amount)
        {
            res = this.res;
            amount = this.amount;
        }

        public AreaInt Area()
            => amount * res.Area;

        public static ResAmount<TRes> operator *(ulong left, ResAmount<TRes> right)
            => new(res: right.res, amount: left * right.amount);

        public static ResAmount<TRes> operator *(ResAmount<TRes> left, ulong right)
            => right * left;
    }
}
