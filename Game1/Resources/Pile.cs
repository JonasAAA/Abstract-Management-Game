namespace Game1.Resources
{
    public class Pile<TAmount> : IPile<TAmount>
        where TAmount : struct, ICountable<TAmount>
    {
        public LocationCounters LocationCounters { get; }

        public TAmount Amount
            => counter.Count;

        private readonly Counter<TAmount> counter;

        public void TransferTo(IndividualCounters destin, TAmount amount)
        {
            throw new NotImplementedException();
        }

        public void TransferFrom(IndividualCounters source, TAmount amount)
        {
            throw new NotImplementedException();
        }
    }
}
