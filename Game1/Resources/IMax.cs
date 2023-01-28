namespace Game1.Resources
{
    public interface IMax<T>
        where T : IMax<T>
    {
        public abstract static T Max(T left, T right);
    }
}
