﻿namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResRecipe
    {
        public static ResRecipe CreateOrThrow(AllResAmounts ingredients, AllResAmounts results)
        {
            if (AreValid(ingredients: ingredients, results: results))
                return new(ingredients: ingredients, results: results);
            throw new ArgumentException();
        }

        public bool IsEmpty
            => ingredients.IsEmpty;

        public readonly AllResAmounts ingredients, results;

        private ResRecipe(AllResAmounts ingredients, AllResAmounts results)
        {
            Debug.Assert(AreValid(ingredients: ingredients, results: results));
            Debug.Assert(ingredients.Area() == results.Area());
            Debug.Assert(ingredients.HeatCapacity() == results.HeatCapacity());
            Debug.Assert(ingredients.Mass() == results.Mass());
            this.ingredients = ingredients;
            this.results = results;
        }

        private static bool AreValid(AllResAmounts ingredients, AllResAmounts results)
            => ingredients.RawMatComposition() == results.RawMatComposition();

        public static ResRecipe operator *(ulong scalar, ResRecipe resRecipe)
            => new(ingredients: scalar * resRecipe.ingredients, results: scalar * resRecipe.results);

        public static ResRecipe operator *(ResRecipe resRecipe, ulong scalar)
            => scalar * resRecipe;
    }
}

// TODO: could create energy recipe with something like
//namespace Game1.Resources
//{
//    public class EnergyRecipe<TAmount, TAmount>
//        where TAmount : struct, IFormOfEnergy<TAmount>
//        where TAmount : struct, IFormOfEnergy<TAmount>
//    {
//        public static EnergyRecipe<TAmount, TAmount>? CreateOrThrow(TAmount ingredients, TAmount results)
//        {
//            if (AreValid(ingredients: ingredients, results: results))
//                return new(ingredients: ingredients, results: results);
//            return null;
//        }

//        public readonly TAmount ingredients;
//        public readonly TAmount results;

//        private EnergyRecipe(TAmount ingredients, TAmount results)
//        {
//            "Do I make stuff use this instead of current energy conversion methods?
//            Debug.Assert(AreValid(ingredients: ingredients, results: results));
//            this.ingredients = ingredients;
//            this.results = results;
//        }

//        private static bool AreValid(TAmount ingredients, TAmount results)
//            => (Energy)ingredients == (Energy)results;
//    }
//}

