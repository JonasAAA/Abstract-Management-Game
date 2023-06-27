//namespace Game1.Industries
//{
//    // TODO: make people unhappy if have unwanted resources
//    [Serializable]
//    public class BuildingShape
//    {
//        public SomeResAmounts<IResource> Cost
//            => resPile.Amount;

//        public readonly Mass splittingMass;

//        private readonly ResPile resPile;

//        public BuildingShape(ResPile resSource)
//        {
//            if (resSource.Amount.IsEmpty())
//                throw new ArgumentException();
//            resPile = resSource;
//            splittingMass = Cost.Mass();
//        }

//        public void Delete(ResPile resDestin)
//            => resDestin.TransferAllFrom(source: resPile);
//    }
//}
