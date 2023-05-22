using System.Collections;

namespace Game1.Collections
{
    /// <summary>
    /// Just like ReadOnlyCollection, but it's struct instead of class and all function calls are direct instead of virtual
    /// </summary>
    [Serializable]
    public readonly struct EfficientReadOnlyCollection<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> list;

        public EfficientReadOnlyCollection()
            => list = new();

        public EfficientReadOnlyCollection(List<T> list)
            => this.list = list;

        public int Count
            => list.Count;

        public IEnumerator<T> GetEnumerator()
            => list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => list.GetEnumerator();
    }
}
