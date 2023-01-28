using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    // TODO: make people unhappy if have unwanted resources
    [Serializable]
    public class Building
    {
        public ResAmounts Cost
            => resPile.Amount;

        public readonly Mass mass;

        private readonly ResPile resPile;

        public Building(ResPile resSource)
        {
            if (resSource.Amount.IsEmpty())
                throw new ArgumentException();
            resPile = resSource;
            mass = Cost.Mass();
        }

        public void Delete(ResPile resDestin)
            => resDestin.TransferAllFrom(source: resPile);
    }
}
