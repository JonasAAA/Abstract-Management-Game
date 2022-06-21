using System.Diagnostics.CodeAnalysis;

namespace Game1.Resources
{
    [Serializable]
    public class ReservedResPile : ResPileBase
    {
        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmounts resAmounts)
        {
            if (source.ResAmounts >= resAmounts)
            {
                ReservedResPile resPile = new();
                Transfer(source: source, destin: resPile, resAmounts: resAmounts);
                return resPile;
            }
            return null;
        }

        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmount resAmount)
            => CreateIfHaveEnough(source: source, resAmounts: new(resAmount: resAmount));

        public static ReservedResPile CreateFromSource([DisallowNull] ref ReservedResPile? source)
        {
            ReservedResPile resPile = new();
            TransferAll(source: source, destin: resPile);
            source = null;
            return resPile;
        }

        public static void TransferAll([DisallowNull] ref ReservedResPile? reservedSource, ResPile destin)
        {
            Transfer(source: reservedSource, destin: destin, resAmounts: reservedSource.ResAmounts);
            reservedSource = null;
        }

        private ReservedResPile()
            : base()
        { }
    }
}
