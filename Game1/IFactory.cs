namespace Game1
{
    public interface IFactory<TParams, TValue>
    {
        public TValue Create(TParams parameters);
    }
}
