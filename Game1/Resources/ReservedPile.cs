namespace Game1.Resources
{
    [Serializable]
    public class ReservedPile<TAmount> : ISourcePile<TAmount>
        where TAmount : struct, ICountable<TAmount>
    {
        public TAmount Amount
            => pile.Amount;

        TAmount ISourcePile<TAmount>.Amount => throw new NotImplementedException();

        LocationCounters IPileBase.LocationCounters => throw new NotImplementedException();

        private readonly Pile<TAmount> pile;

        public static ReservedPile<TAmount>? CreateIfHaveEnough<TSourePile>(TSourePile source, TAmount amount)
            where TSourePile : ISourcePile<TAmount>
        {
            if (source.Amount >= amount)
            {
                ReservedPile<TAmount> reservedPile = new(locationCounters: source.LocationCounters);
                reservedPile.pile.TransferFrom(source: source, amount: amount);
                return reservedPile;
            }
            return null;
        }

        //public static ReservedPile<TAmount> CreateFromSource([DisallowNull] ref ReservedPile<TAmount>? source)
        //{
        //    ReservedPile<TAmount> resPile = new(locationCounters: source.LocationCounters);
        //    TransferAllFrom(source: source, destin: resPile);
        //    source = null;
        //    return resPile;
        //}

        private ReservedPile(LocationCounters locationCounters)
            => pile = Pile<TAmount>.CreateEmpty(locationCounters: locationCounters);

        public void ChangeLocation(LocationCounters newLocationCounters)
            => pile.ChangeLocation(newLocationCounters: newLocationCounters);

        public void TransferAllTo<TDestinPile>(TDestinPile destin)
            where TDestinPile : IDestinPile<TAmount>
            => pile.TransferAllTo(destin: destin);

        void ISourcePile<TAmount>.TransferAllTo(Pile<TAmount> destin)
            => pile.TransferAllTo(destin: destin);
    }
}

//namespace Game1.Resources
//{
//    [Serializable]
//    public class ReservedPile<TAmount>
//        where TAmount : struct, ICountable<TAmount>
//    {
//        public TAmount Amount
//            => pile.Amount;

//        private readonly Pile<TAmount> pile;

//        public static ReservedPile<TAmount>? CreateIfHaveEnough(Pile<TAmount> source, TAmount amount)
//        {
//            if (source.Amount >= amount)
//            {
//                ReservedPile<TAmount> reservedPile = new(locationCounters: source.LocationCounters);
//                reservedPile.pile.TransferFrom(source: source, amount: amount);
//                return reservedPile;
//            }
//            return null;
//        }

//        //public static ReservedPile<TAmount> CreateFromSource([DisallowNull] ref ReservedPile<TAmount>? source)
//        //{
//        //    ReservedPile<TAmount> resPile = new(locationCounters: source.LocationCounters);
//        //    TransferAllFrom(source: source, destin: resPile);
//        //    source = null;
//        //    return resPile;
//        //}

//        private ReservedPile(LocationCounters locationCounters)
//            => pile = Pile<TAmount>.CreateEmpty(locationCounters: locationCounters);

//        public void TransferAllTo(Pile<TAmount> destin)
//            => pile.TransferAllTo(destin: destin);
//    }
//}

//using System.Diagnostics.CodeAnalysis;

//namespace Game1.Resources
//{
//    [Serializable]
//    public class ReservedResPile : ResPileBase
//    {
//        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmounts resAmounts)
//        {
//            if (source.ResAmounts >= resAmounts)
//            {
//                ReservedResPile resPile = new(locationCounters: source.LocationCounters);
//                Transfer(source: source, destin: resPile, resAmounts: resAmounts);
//                return resPile;
//            }
//            return null;
//        }

//        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmount resAmount)
//            => CreateIfHaveEnough(source: source, resAmounts: new(resAmount: resAmount));

//        public static ReservedResPile CreateFromSource([DisallowNull] ref ReservedResPile? source)
//        {
//            ReservedResPile resPile = new(locationCounters: source.LocationCounters);
//            TransferAllFrom(source: source, destin: resPile);
//            source = null;
//            return resPile;
//        }

//        private ReservedResPile(LocationCounters locationCounters)
//            : base(locationCounters: locationCounters)
//        { }
//    }
//}
