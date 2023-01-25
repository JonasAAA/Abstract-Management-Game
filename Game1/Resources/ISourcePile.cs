namespace Game1.Resources
{
    public interface ISourcePile<TAmount> : IPileBase
        where TAmount : struct, ICountable<TAmount>
    {
        public TAmount Amount { get; }

        //public void TransferTo(Pile<TAmount> destin, TAmount amount);

        public void TransferAllTo(Pile<TAmount> destin);
    }
}
