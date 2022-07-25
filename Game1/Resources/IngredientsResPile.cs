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

        public readonly ResRecipe recipe;

        private IngredientsResPile(ResPile source, ResRecipe recipe)
            : base(locationMassCounter: source.LocationMassCounter)
        {
            Transfer(source: source, destin: this, resAmounts: recipe.ingredients);
            this.recipe = recipe;
        }
    }
}
