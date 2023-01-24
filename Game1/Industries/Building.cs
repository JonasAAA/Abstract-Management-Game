using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    // TODO: make people unhappy if have unwanted resources
    [Serializable]
    public class Building
    {
        public ResAmounts Cost
            => ResPile.Amount;

        public readonly Mass mass;

        private ReservedPile<ResAmounts> ResPile
            => resPile ?? throw new InvalidOperationException(buildingIsDeletedMessage);

        private readonly ReservedPile<ResAmounts> resPile;
        private const string buildingIsDeletedMessage = "building has been deleted";

        public Building(ReservedPile<ResAmounts> resSource)
        {
            if (resSource.Amount.IsEmpty())
                throw new ArgumentException();
            resPile = resSource;
            mass = Cost.Mass();
        }

        public void Delete<TDestinPile>(TDestinPile resDestin)
            where TDestinPile : IDestinPile<ResAmounts>
        {
            if (resPile is null)
                throw new InvalidOperationException(buildingIsDeletedMessage);
            resDestin.TransferAllFrom(source: resPile);
        }
    }
}
