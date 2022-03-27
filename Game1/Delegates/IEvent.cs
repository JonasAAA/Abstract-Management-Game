namespace Game1.Delegates
{
    public interface IEvent<T>
        where T : IListener
    {
        public bool Contains(T listener);

        public void Add(T listener);

        public void Remove(T listener);
    }
}
