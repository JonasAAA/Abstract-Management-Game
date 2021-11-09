using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game1
{
    /// <summary>
    /// throws exception when duplicates are added or try to remove non-existent element
    /// </summary>
    [CollectionDataContract]
    public class MyHashSet<T> : IEnumerable<T>
    {
        public int Count
            => set.Count;

        [DataMember] private readonly HashSet<T> set;
        
        public MyHashSet()
            => set = new();

        public MyHashSet(IEnumerable<T> collection)
            : this()
        {
            UnionWith(collection: collection);
        }

        public bool Contains(T item)
            => set.Contains(item);

        public void Add(T item)
        {
            if (!set.Add(item))
                throw new ArgumentException();
        }

        public void UnionWith(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Add(item: item);
        }

        public void Remove(T item)
        {
            if (!set.Remove(item))
                throw new ArgumentException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Remove(item: item);
        }

        public void Clear()
            => set.Clear();

        public IEnumerator<T> GetEnumerator()
            => set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
