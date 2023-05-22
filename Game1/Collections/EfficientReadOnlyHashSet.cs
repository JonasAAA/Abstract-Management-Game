using System.Collections;

namespace Game1.Collections
{
    public readonly struct EfficientReadOnlyHashSet<T> : IReadOnlySet<T>
    {
        public int Count
            => set.Count;

        private readonly HashSet<T> set;

        public EfficientReadOnlyHashSet()
            : this(set: new())
        { }

        public EfficientReadOnlyHashSet(T value)
            : this(set: new() { value })
        { }

        public EfficientReadOnlyHashSet(HashSet<T> set)
            => this.set = set;

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
