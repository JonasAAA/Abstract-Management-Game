namespace Game1
{
    [Serializable]
    public class CachedValue<T>
    {
        private T value;
        private TimeSpan setTime;

        public CachedValue()
        {
            value = default!;
            setTime = TimeSpan.MinValue;
        }

        public T Get(Func<T> computeValue, TimeSpan curTime)
        {
            if (setTime != curTime)
            {
                value = computeValue();
                setTime = curTime;
            }
            return value;
        }
    }
}
