using Game1.Events;

namespace Game1
{
    public interface IDeletable
    {
        public IEvent<IDeletedListener> Deleted { get; }
    }
}
