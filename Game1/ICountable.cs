namespace Game1
{
    public interface ICountable<T>
    {
        public bool IsZero { get; }

        public T Add(T count);

        public T Subtract(T count);
    }
}
