namespace Game1.ChangingValues
{
    public interface ITransformer<TParam, TResult>
    {
        public TResult Transform(TParam param);
    }
}
