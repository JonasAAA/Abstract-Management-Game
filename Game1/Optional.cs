namespace Game1
{
    // TODO: delete
    [Serializable]
    public readonly ref struct Optional<TValue>
        where TValue : struct
    {
        private readonly TValue value;
        private readonly bool exists;

        public Optional(TValue value)
        {
            this.value = value;
            exists = true;
        }

        public Optional()
        {
            value = default;
            exists = false;
        }

        public void Process(Action<TValue> processValue, Action processNone)
        {
            if (exists)
                processValue(value);
            else
                processNone();
        }

        public T Process<T>(Func<TValue, T> processValue, Func<T> processNone)
        {
            if (exists)
                return processValue(value);
            else
                return processNone();
        }
    }
}
