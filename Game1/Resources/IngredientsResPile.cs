using System.Diagnostics.CodeAnalysis;

namespace Game1.Resources
{
    [Serializable]
    public class IngredientsResPile : ResPileBase
    {
        public static IngredientsResPile? CreateIfHaveEnough(ResPile source, ResRecipe recipe)
        {
            if (source.ResAmounts >= recipe.ingredients)
                return new(source: source, recipe: recipe);
            return null;
        }

        public static void TransformAndTransferAll([DisallowNull] ref IngredientsResPile? ingredients, ResPile destin)
        {
            ingredients.Transform(recipe: ingredients.recipe);
            TransferAll(source: ingredients, destin: destin);
            ingredients = null;
        }

        private readonly ResRecipe recipe;

        private IngredientsResPile(ResPile source, ResRecipe recipe)
            : base()
        {
            Transfer(source: source, destin: this, resAmounts: recipe.ingredients);
            this.recipe = recipe;
        }
    }
}
