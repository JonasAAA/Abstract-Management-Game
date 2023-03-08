namespace Game1.Resources
{
    public interface IMin<T>
        where T : IMin<T>
    {
        public abstract static T Min(T left, T right);
    }
}
