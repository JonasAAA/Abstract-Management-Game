namespace Game1.Delegates
{
    public interface IAction
    {
        public void Invoke();
    }

    public interface IAction<T>
    {
        public void Invoke(T value);
    }
}
