//namespace Game1.Industries
//{
//    // TODO: make people unhappy if have unwanted resources
//    [Serializable]
//    public class Building
//    {
//        public SomeResAmounts<IResource> Cost
//            => resPile.Amount;

//        public readonly Mass miningMass;

//        private readonly ResPile resPile;

//        public Building(ResPile resSource)
//        {
//            if (resSource.Amount.IsEmpty())
//                throw new ArgumentException();
//            resPile = resSource;
//            miningMass = Cost.Mass();
//        }

//        public void Delete(ResPile resDestin)
//            => resDestin.TransferAllFrom(source: resPile);
//    }
//}
