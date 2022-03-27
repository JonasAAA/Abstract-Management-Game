using Game1.Delegates;

namespace Game1
{
    public interface IDeletable
    {
        public IEvent<IDeletedListener> Deleted { get; }
    }
}
