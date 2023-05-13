namespace Game1.Resources
{
    /// <summary>
    /// The major difference to Dictionary is that when key doesn't exist on lookup, instead of throwing an error, it creates the key with new() value.
    /// </summary>
    [Serializable]
    public sealed class MyDict<TKey, TValue>
        where TKey : notnull
        where TValue : class, new()
    {
        public Dictionary<TKey, TValue>.ValueCollection Values
            => dict.Values;

        private readonly Dictionary<TKey, TValue> dict;

        public MyDict()
            => dict = new();

        public TValue GetOrCreate(TKey key)
            => dict.TryGetValue(key, out var value) switch
            {
                true => value,
                false => dict[key] = new()
            };
    }

    ///// <summary>
    ///// The major difference to Dictionary is that when key doesn't exist on lookup, instead of throwing an error, it creates the key with new() value.
    ///// </summary>
    //public class MyDict<TRes, TValue>
    //    where TRes : notnull
    //    where TValue : class
    //{
    //    public Dictionary<TRes, TValue>.ValueCollection Values
    //        => dict.Values;

    //    private readonly IFactory<TRes, TValue> valueFactory;
    //    private readonly Dictionary<TRes, TValue> dict;

    //    public MyDict(IFactory<TRes, TValue> valueFactory)
    //    {
    //        this.valueFactory = valueFactory;
    //        dict = new();
    //    }

    //    public TValue GetOrCreate(TRes key)
    //        => dict.TryGetValue(key, out var value) switch
    //        {
    //            true => value,
    //            false => dict[key] = valueFactory.Create(parameters: key)
    //        };
    //}
}
