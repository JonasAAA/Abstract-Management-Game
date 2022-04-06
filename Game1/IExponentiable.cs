namespace Game1
{
    public interface IExponentiable<TExponent, TResult>
    {
        public TResult Pow(TExponent exponent);
    }
}
