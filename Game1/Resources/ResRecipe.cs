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
            Debug.Assert(AreValid(ingredients: ingredients, results: results));
            this.ingredients = ingredients;
            this.results = results;
        }

        private static bool AreValid(ResAmounts ingredients, ResAmounts results)
            => ingredients.ConvertToBasic() == results.ConvertToBasic();

        public static ResRecipe operator *(ulong scalar, ResRecipe resRecipe)
            => new(ingredients: scalar * resRecipe.ingredients, results: scalar * resRecipe.results);

        public static ResRecipe operator *(ResRecipe resRecipe, ulong scalar)
            => scalar * resRecipe;
    }
}

// TODO: could create energy recipe with something like
//namespace Game1.Resources
//{
//    public class EnergyRecipe<TSourceAmount, TDestinAmount>
//        where TSourceAmount : struct, IFormOfEnergy<TSourceAmount>
//        where TDestinAmount : struct, IFormOfEnergy<TDestinAmount>
//    {
//        public static EnergyRecipe<TSourceAmount, TDestinAmount>? Create(TSourceAmount ingredients, TDestinAmount results)
//        {
//            if (AreValid(ingredients: ingredients, results: results))
//                return new(ingredients: ingredients, results: results);
//            return null;
//        }

//        public readonly TSourceAmount ingredients;
//        public readonly TDestinAmount results;

//        private EnergyRecipe(TSourceAmount ingredients, TDestinAmount results)
//        {
//            "Do I make stuff use this instead of current energy conversion methods?
//            Debug.Assert(AreValid(ingredients: ingredients, results: results));
//            this.ingredients = ingredients;
//            this.results = results;
//        }

//        private static bool AreValid(TSourceAmount ingredients, TDestinAmount results)
//            => (Energy)ingredients == (Energy)results;
//    }
//}

