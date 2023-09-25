using System.Collections;

namespace Game1.Collections
{
    /// <summary>
    /// throws exception when duplicates are added or try to remove non-existent element
    /// </summary>
    //[CollectionDataContract]
    [Serializable]
    public sealed class ThrowingSet<T> : IEnumerable<T>
    {
        public ulong Count
            => (ulong)set.Count;

        private readonly HashSet<T> set;

        public ThrowingSet()
            => set = new();

        public ThrowingSet(IEnumerable<T> collection)
            : this()
            => UnionWith(collection: collection);

        public bool Contains(T item)
            => set.Contains(item);

        public void Add(T item)
        {
            if (!set.Add(item))
                throw new ArgumentException();
        }

        public void TryAdd(T item)
            => set.Add(item);

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
