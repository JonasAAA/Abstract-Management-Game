namespace Game1.Resources
{
    public interface ISourcePile<TAmount> : IPileBase
        where TAmount : struct, ICountable<TAmount>
    {
        public TAmount Amount { get; }

        public void TransferTo(IndividualCounters destin, TAmount amount);
    }
}
