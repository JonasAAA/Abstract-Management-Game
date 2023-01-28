using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public class Counter<TAmount>
        where TAmount : struct, ICountable<TAmount>
    {
        public static Counter<TAmount> CreateEmpty()
            => new(createdByMagic: false);

        public static Counter<TAmount> CreateByMagic(TAmount count)
        {
            
            return new(createdByMagic: true)
            {
                Count = count
            };
        }

        public TAmount Count { get; protected set; }
#if DEBUG2
        private readonly bool createdByMagic;
#endif

        protected Counter(bool createdByMagic)
        {
            if (createdByMagic && typeof(TAmount) != typeof(NumPeople) && CurWorldManager.CurTime != CurWorldManager.StartTime)
                throw new Exception("Can only create non-people counters by magic at the very start of the game");
            Count = TAmount.AdditiveIdentity;
#if DEBUG2
            this.createdByMagic = createdByMagic;
#endif
        }

        public void TransferFrom(Counter<TAmount> source, TAmount count)
        {
            if (source == this)
                return;
            source.Count -= count;
            Count += count;
        }

        public void TransferTo(Counter<TAmount> destin, TAmount count)
            => destin.TransferFrom(source: this, count: count);

#if DEBUG2
        ~Counter()
        {
            if (!createdByMagic && Count != TAmount.AdditiveIdentity)
                throw new Exception();
        }
#endif
    }
}
