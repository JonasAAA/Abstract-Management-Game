using System.Collections;
using System.Runtime.CompilerServices;

namespace Game1.Collections
{

    /// <summary>
    /// Immutable dictionary where keys are (TRes)0, (TRes)1, ...
    /// </summary>
    /// <typeparam Name="TKey">must be integer-backed enum</typeparam>
    [Serializable]
    public readonly struct EnumDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, Enum
    {
        public IEnumerable<TValue> Values
            => values;

        private readonly TValue[] values;

        public EnumDict()
            => values = new TValue[Enum.GetValues<TKey>().Length];

        public EnumDict(TValue value)
            : this()
            => Array.Fill(values, value);

        public EnumDict(Func<TKey, TValue> selector)
            => values = Enum.GetValues<TKey>().Select(selector).ToArray();

        private EnumDict<TKey, TValue> Clone()
        {
            EnumDict<TKey, TValue> newEnumDict = new();
            values.CopyTo(newEnumDict.values, index: 0);
            return newEnumDict;
        }

        public EnumDict<TKey, TValue> Update(IEnumerable<(TKey, TValue)> newValues)
        {
            EnumDict<TKey, TValue> newEnumDict = Clone();
            foreach (var (key, newValue) in newValues)
                newEnumDict[key] = newValue;
            return newEnumDict;
        }

        public EnumDict<TKey, TValue> Update(TKey key, TValue newValue)
        {
            EnumDict<TKey, TValue> newEnumDict = Clone();
            newEnumDict[key] = newValue;
            return newEnumDict;
        }

        public TValue this[TKey key]
        {
            get => values[KeyToIndex(key)];
            private set => values[KeyToIndex(key)] = value;
        }

        private static int KeyToIndex(TKey key)
        {
            // implementation taken from https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104840
            // The enum must be backed by int, so that conversion makes sense
            // This is very fast as explained here https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104851
            if (Unsafe.SizeOf<TKey>() != Unsafe.SizeOf<int>())
                throw new InvalidOperationException("type mismatch");
            return Unsafe.As<TKey, int>(ref key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in Enum.GetValues<TKey>())
                yield return new KeyValuePair<TKey, TValue>(key: key, value: this[key]);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
