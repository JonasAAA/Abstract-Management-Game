namespace Game1.Resources
{
    public interface IDestinPile<TAmount> : IPileBase
        where TAmount : struct, ICountable<TAmount>
    {
        public void TransferFrom(IndividualCounters source, TAmount amount);
    }
}
