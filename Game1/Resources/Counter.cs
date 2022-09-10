global using MassCounter = Game1.Resources.Counter<Game1.Resources.Mass>;
global using PeopleCounter = Game1.Resources.Counter<Game1.Resources.NumPeople>;

namespace Game1.Resources
{
    public interface ICountable<T>
    {
        public bool IsZero { get; }

        public T Add(T count);

        public T Subtract(T count);
    }

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

        public T Count { get; private set; }
#if DEBUG2
        private readonly bool createdByMagic;
#endif

        private Counter(bool createdByMagic)
        {
            Count = default;
#if DEBUG2
            this.createdByMagic = createdByMagic;
#endif
        }

        public void TransferFrom(Counter<T> source, T count)
        {
            if (source == this)
                return;
            source.Count = source.Count.Subtract(count: count);
            Count = Count.Add(count: count);
        }

#if DEBUG2
        ~Counter()
        {
            if (!createdByMagic && !Count.IsZero)
                throw new Exception();
        }
#endif
    }
}
