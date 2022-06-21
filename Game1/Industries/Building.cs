using System.Diagnostics.CodeAnalysis;

namespace Game1.Industries
{
    // TODO: make people unhappy if have unwanted resources
    [Serializable]
    public class Building
    {
        public ResAmounts Cost
            => ResPile.ResAmounts;
        // TODO: should probably have a separate type for mass
        public readonly ulong mass;

        private ReservedResPile ResPile
            => resPile ?? throw new InvalidOperationException(buildingIsDeletedMessage);

        private ReservedResPile? resPile;
        private const string buildingIsDeletedMessage = "building has been deleted";

        public Building([DisallowNull] ref ReservedResPile? resSource)
        {
            if (resSource.IsEmpty)
                throw new ArgumentException();
            resPile = ReservedResPile.CreateFromSource(source: ref resSource);
            mass = Cost.TotalMass();
        }

        public static void Delete([DisallowNull] ref Building? building, ResPile resDestin)
        {
            if (building.resPile is null)
                throw new InvalidOperationException(buildingIsDeletedMessage);
            ReservedResPile.TransferAll(reservedSource: ref building.resPile, destin: resDestin);
            building = null;
        }
    }
}
