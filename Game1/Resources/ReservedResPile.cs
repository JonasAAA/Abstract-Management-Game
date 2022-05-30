using System.Diagnostics.CodeAnalysis;

namespace Game1.Resources
{
    [Serializable]
    public class ReservedResPile : ResPileBase
    {
        public static ReservedResPile? Create(ResPile source, ResAmounts resAmounts)
        {
            if (source.ResAmounts >= resAmounts)
            {
                ReservedResPile resPile = new();
                Transfer(source: source, destin: resPile, resAmounts: resAmounts);
                return resPile;
            }
            return null;
        }

        public static ReservedResPile? Create(ResPile source, ResAmount resAmount)
            => Create(source: source, resAmounts: new(resAmount: resAmount));

        public static ReservedResPile Create([DisallowNull] ref ReservedResPile? source)
        {
            ReservedResPile resPile = new();
            TransferAll(source: source, destin: resPile);
            source = null;
            return resPile;
        }

        // TODO: delete if unused
        //public static ReservedResPile Create([DisallowNull] ref IngredientsResPile? transformSource)
        //{
        //    ReservedResPile resPile = new();
        //    TransferAll(source: transformSource, destin: resPile);
        //    resPile.Transform(recipe: transformSource.recipe);
        //    transformSource = null;
        //    return resPile;
        //}

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
