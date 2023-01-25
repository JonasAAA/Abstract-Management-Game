namespace Game1.Resources
{
    public class Pile<TAmount> : ISourcePile<TAmount>, IDestinPile<TAmount>
        where TAmount : struct, ICountable<TAmount>
    {
        public static Pile<TAmount> CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters, counter: Counter<TAmount>.CreateEmpty());

        public TAmount Amount
            => Counter.Count;

        public bool IsEmpty
            => Amount == TAmount.AdditiveIdentity;
        
        public LocationCounters LocationCounters { get; private set; }
        /// <summary>
        /// THIS MUST always be used together with LocationCounters
        /// </summary>
        protected virtual Counter<TAmount> Counter { get; }

        protected Pile(LocationCounters locationCounters, Counter<TAmount> counter)
        {
            LocationCounters = locationCounters;
            Counter = counter;
        }

        public void ChangeLocation(LocationCounters newLocationCounters)
        {
            newLocationCounters.TransferFrom(source: LocationCounters, amount: Amount);
            LocationCounters = newLocationCounters;
        }

        public void TransferFrom<TSourcePile>(TSourcePile source, TAmount amount)
            where TSourcePile : ISourcePile<TAmount>
            => source.TransferTo(destin: this, amount: amount);

        public void TransferAllFrom<TSourcePile>(TSourcePile source)
            where TSourcePile : ISourcePile<TAmount>
            => source.TransferAllTo(destin: this);

        public void TransferTo<TDestinPile>(TDestinPile destin, TAmount amount)
            where TDestinPile : IDestinPile<TAmount>
            => destin.TransferFrom(source: this, amount: amount);

        public void TransferAllTo<TDestinPile>(TDestinPile destin)
            where TDestinPile : IDestinPile<TAmount>
            => destin.TransferAllFrom(source: this);

        private void TransferFrom(Pile<TAmount> source, TAmount amount)
        {
            Counter.TransferFrom(source: source.Counter, count: amount);
            LocationCounters.TransferFrom(source: source.LocationCounters, amount: amount);
        }

        private void TransferTo(Pile<TAmount> destin, TAmount amount)
            => destin.TransferFrom(source: this, amount: amount);

        void IDestinPile<TAmount>.TransferFrom(Pile<TAmount> source, TAmount amount)
            => TransferFrom(source: source, amount: amount);

        void IDestinPile<TAmount>.TransferAllFrom(Pile<TAmount> source)
            => TransferFrom(source: source, amount: source.Amount);

        void IDestinPile<TAmount>.TransferAllFrom(ReservedPile<TAmount> source)
            => source.TransferAllTo(destin: this);

        //void ISourcePile<TAmount>.TransferTo(Pile<TAmount> destin, TAmount amount)
        //    => TransferTo(destin: destin, amount: amount);

        void ISourcePile<TAmount>.TransferAllTo(Pile<TAmount> destin)
            => TransferTo(destin: destin, amount: Amount);
    }
}

//namespace Game1.Resources
//{
//    public class Pile<TAmount> : ISourcePile<TAmount>, IDestinPile<TAmount>
//        where TAmount : struct, ICountable<TAmount>
//    {
//        public static Pile<TAmount> CreateEmpty(LocationCounters locationCounters)
//            => new(locationCounters: locationCounters, counter: Counter<TAmount>.CreateEmpty());

//        public LocationCounters LocationCounters { get; }

//        public TAmount Amount
//            => Counter.Count;

//        public bool IsEmpty
//            => Amount == TAmount.AdditiveIdentity;

//        protected virtual Counter<TAmount> Counter { get; }

//        private Pile(LocationCounters locationCounters, Counter<TAmount> counter)
//        {
//            LocationCounters = locationCounters;
//            Counter = counter;
//        }

//        public void TransferFrom(Pile<TAmount> source, TAmount amount)
//            => Counter.TransferFrom(source: source.Counter, count: amount);

//        public void TransferTo(Pile<TAmount> destin, TAmount amount)
//            => Counter.TransferTo(destin: destin.Counter, count: amount);

//        public void TransferAllFrom(Pile<TAmount> source)
//            => TransferFrom(source: source, amount: source.Amount);

//        public void TransferAllTo(Pile<TAmount> destin)
//            => TransferTo(destin: destin, amount: Amount);
//    }
//}
