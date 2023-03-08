namespace Game1.Delegates
{
    [Serializable]
    public sealed class Event<T> : IEvent<T>
        where T : IListener
    {
        private readonly MySet<T> listeners;

        public Event()
            => listeners = new();

        public bool Contains(T listener)
            => listeners.Contains(listener);

        public void Add(T listener)
            => listeners.Add(listener);

        /// <summary>
        /// Add if such doesn't already exist
        /// </summary>
        public void TryAdd(T listener)
            => listeners.TryAdd(listener);

        public void Remove(T listener)
            => listeners.Remove(listener);

        public void Raise(Action<T> action)
        {
            foreach (var listener in listeners)
                action(listener);
        }
    }
}
