using System;


namespace Game1.Events
{
    [Serializable]
    public class Event<T> : IEvent<T>
        where T : IListener
    {
        private readonly Dictionary<T> listeners;

        public Event()
            => listeners = new();

        public bool Contains(T listener)
            => listeners.Contains(listener);

        public void Add(T listener)
            => listeners.Add(listener);

        public void Remove(T listener)
            => listeners.Remove(listener);

        public void Raise(Action<T> action)
        {
            foreach (var listener in listeners)
                action(listener);
        }
    }
}
