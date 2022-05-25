namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResRecipe
    {
        public static ResRecipe? Create(ResAmounts ingredients, ResAmounts results)
        {
            if (AreValid(ingredients: ingredients, results: results))
                return new(ingredients: ingredients, results: results);
            return null;
        }

        public bool IsEmpty
            => ingredients.IsEmpty();

        public readonly ResAmounts ingredients, results;

        private ResRecipe(ResAmounts ingredients, ResAmounts results)
        {
            this.ingredients = ingredients;
            this.results = results;
            Debug.Assert(AreValid(ingredients: ingredients, results: results));
        }

        private static bool AreValid(ResAmounts ingredients, ResAmounts results)
            => ingredients.ConvertToBasic() == results.ConvertToBasic();

        public static ResRecipe operator *(ulong scalar, ResRecipe resRecipe)
            => new(ingredients: scalar * resRecipe.ingredients, results: scalar * resRecipe.results);

        public static ResRecipe operator *(ResRecipe resRecipe, ulong scalar)
            => scalar * resRecipe;
    }
}
