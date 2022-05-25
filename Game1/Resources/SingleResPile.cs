// TODO: delete if unused, implement if want to used
//namespace Game1.Resources
//{
//    public class SingleResPile
//    {
//        public static SingleResPile CreateEmpty()
//            => new();

//        public ulong TotalMass { get; private set; }
//        public bool IsEmpty
//            => TotalMass is 0;
//        public ResAmounts ResAmounts { get; private set; }

//        private ResPile()
//            => ResAmounts = new();

//        public ulong this[ResInd resInd]
//            => ResAmounts[resInd];

//        public static void TransferAll(ResPile source, ResPile destin)
//            => Transfer(source: source, destin: destin, resAmounts: source.ResAmounts);

//        public static void Transfer(ResPile source, ResPile destin, ResAmounts resAmounts)
//        {
//            ulong transferMass = resAmounts.TotalMass();

//            source.ResAmounts -= resAmounts;
//            source.TotalMass -= transferMass;

//            destin.ResAmounts += resAmounts;
//            destin.TotalMass += transferMass;
//        }

//        public static void Transfer(ResPile source, ResPile destin, ResAmount resAmount)
//            => Transfer
//            (
//                source: source,
//                destin: destin,
//                resAmounts: new ResAmounts(resAmount: resAmount)
//            );

//#if DEBUG
//        ~SingleResPile()
//        {
//            if (!ResAmounts.IsEmpty())
//                throw new Exception($"can only discard empty {nameof(ResPile)}");
//        }
//#endif
//    }
//}
