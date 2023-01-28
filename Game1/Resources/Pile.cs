namespace Game1.Resources
{
    [Serializable]
    public class Pile<TAmount>
        where TAmount : struct, ICountable<TAmount>
    {
        public static Pile<TAmount> CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters, counter: Counter<TAmount>.CreateEmpty());

        public static Pile<TAmount>? CreateIfHaveEnough(Pile<TAmount> source, TAmount amount)
        {
            if (source.Amount >= amount)
            {
                Pile<TAmount> newPile = new(locationCounters: source.LocationCounters, counter: Counter<TAmount>.CreateEmpty());
                newPile.TransferFrom(source: source, amount: amount);
                return newPile;
            }
            return null;
        }

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

        public void TransferFrom(Pile<TAmount> source, TAmount amount)
        {
            Counter.TransferFrom(source: source.Counter, count: amount);
            LocationCounters.TransferFrom(source: source.LocationCounters, amount: amount);
        }

        public void TransferTo(Pile<TAmount> destin, TAmount amount)
            => destin.TransferFrom(source: this, amount: amount);

        public void TransferAllFrom(Pile<TAmount> source)
            => TransferFrom(source: source, amount: source.Amount);

        public void TransferAllTo(Pile<TAmount> destin)
            => TransferTo(destin: destin, amount: Amount);

        public void TransferAtMostFrom(Pile<TAmount> source, TAmount maxAmount)
            => TransferFrom(source: source, amount: MyMathHelper.Min(maxAmount, Amount));
    }
}

//namespace Game1.Resources
//{
//    public class Pile<TAmount> : ISourcePile<TAmount>, IDestinPile<TAmount>
//        where TAmount : struct, ICountable<TAmount>
//    {
//        public static Pile<TAmount> CreateEmpty(LocationCounters LocationCounters)
//            => new(LocationCounters: LocationCounters, counter: Counter<TAmount>.CreateEmpty());

//        public LocationCounters LocationCounters { get; }

//        public TAmount Amount
//            => Counter.Count;

//        public bool IsEmpty
//            => Amount == TAmount.AdditiveIdentity;

//        protected virtual Counter<TAmount> Counter { get; }

//        private Pile(LocationCounters LocationCounters, Counter<TAmount> counter)
//        {
//            LocationCounters = LocationCounters;
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
