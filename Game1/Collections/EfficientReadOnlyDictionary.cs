using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Game1.Collections
{
    /// <summary>
    /// Just like ReadOnlyDict, but it's struct instead of class and all function calls are direct instead of virtual
    /// </summary>
    [Serializable]
    public readonly struct EfficientReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        public IEnumerable<TKey> Keys
            => dict.Keys;

        public IEnumerable<TValue> Values
            => dict.Values;

        public int Count
            => dict.Count;

        private readonly Dictionary<TKey, TValue> dict;

        public EfficientReadOnlyDictionary()
            => dict = new();

        public EfficientReadOnlyDictionary(Dictionary<TKey, TValue> dict)
            => this.dict = dict;

        public TValue this[TKey key]
        {
            get => dict[key];
            init => dict[key] = value;
        }

        public bool ContainsKey(TKey key)
            => dict.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => dict.GetEnumerator();

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            => dict.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => dict.GetEnumerator();
    }
}
