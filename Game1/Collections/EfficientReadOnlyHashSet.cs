using System.Collections;

namespace Game1.Collections
{
    [Serializable]
    public readonly struct EfficientReadOnlyHashSet<T> : IReadOnlySet<T>
    {
        public static readonly EfficientReadOnlyHashSet<T> empty = new(set: new());

        public int Count
            => set.Count;

        private readonly HashSet<T> set;

        public EfficientReadOnlyHashSet()
            => set = empty.set;

        public EfficientReadOnlyHashSet(T value)
            : this(set: new() { value })
        { }

        public EfficientReadOnlyHashSet(HashSet<T> set)
            => this.set = set;

        public EfficientReadOnlyHashSet<T> Union(EfficientReadOnlyHashSet<T> other)
        {
            if (Count is 0)
                return other;
            if (other.Count is 0)
                return this;
            HashSet<T> set = new();
            set.UnionWith(this);
            set.UnionWith(other);
            return new(set);
        }

        public bool Contains(T item)
            => set.Contains(item);

        public bool IsProperSubsetOf(IEnumerable<T> other)
            => set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other)
            => set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other)
            => set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other)
            => set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other)
            => set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other)
            => set.SetEquals(other);

        public IEnumerator<T> GetEnumerator()
            => set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => set.GetEnumerator();
    }
}
