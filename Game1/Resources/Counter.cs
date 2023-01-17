namespace Game1.Resources
{
    [Serializable]
    public class Counter<T>
        where T : struct, ICountable<T>
    {
        public static Counter<T> CreateEmpty()
            => new(createdByMagic: false);

        public static Counter<T> CreateCounterByMagic(T count)
            => new(createdByMagic: true)
            {
                Count = count
            };

        public T Count { get; protected set; }
#if DEBUG2
        private readonly bool createdByMagic;
#endif

        protected Counter(bool createdByMagic)
        {
            Count = T.AdditiveIdentity;
#if DEBUG2
            this.createdByMagic = createdByMagic;
#endif
        }

        public void TransferFrom(Counter<T> source, T count)
        {
            if (source == this)
                return;
            source.Count -= count;
            Count += count;
        }

#if DEBUG2
        ~Counter()
        {
            if (!createdByMagic && Count != T.AdditiveIdentity)
                throw new Exception();
        }
#endif
    }
}
