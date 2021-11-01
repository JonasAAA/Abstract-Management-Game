using System;
using System.Runtime.Serialization;

namespace Game1.Events
{
    //public interface IEvent<in T>
    //{
    //    public void Add(T listener);

    //    public void Remove(T listener);
    //}

    [DataContract]
    public class Event<T>
    {
        [DataMember]
        private readonly MyHashSet<T> listeners;

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
