//namespace Game1.Resources
//{
//    public interface IEnergyPile<T>
//        where T : struct, IFormOfEnergy<T>
//    {
//        protected T Energy { get; set; }

//        protected LocationCounters LocationCounters { get; }

//        protected static void Transfer(IEnergyPile<T> source, IEnergyPile<T> destin, T energy)
//        {
//            source.Energy -= energy;
//            destin.Energy += energy;
//            destin.LocationCounters.TransferEnergyFrom(source: source, energy: energy);
//        }
//    }
//}
