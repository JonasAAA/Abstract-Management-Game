namespace Game1.MyMath
{
    public interface IExponentiable<TExponent, TResult>
    {
        public TResult Pow(TExponent exponent);
    }
}
