using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class NonBasicRes : IResource
    {
        private const string unitializedExceptionMessage = $"must initialize {nameof(NonBasicRes)} by calling {nameof(Initialize)} first";

        public ulong Mass
            => mass switch
            {
                0 => throw new InvalidOperationException(unitializedExceptionMessage),
                > 0 => mass
            };
        public ResRecipe Recipe
            => recipe switch
            {
                null => throw new InvalidOperationException(unitializedExceptionMessage),
                ResRecipe recipe => recipe
            };

        public ResAmounts BasicIngredients
            => basicIngredients switch
            {
                null => throw new InvalidOperationException(unitializedExceptionMessage),
                ResAmounts resAmounts => resAmounts
            };

        public readonly NonBasicResInd resInd;
        public readonly ResAmounts ingredients;
        private ResRecipe? recipe;
        private ulong mass;
        private ResAmounts? basicIngredients;

        public NonBasicRes(NonBasicResInd resInd, ResAmounts ingredients)
        {
            this.resInd = resInd;
            this.ingredients = ingredients;
            foreach (var otherResInd in ResInd.All)
                if (otherResInd >= resInd && ingredients[otherResInd] > 0)
                    throw new ArgumentException();
            if (ingredients.IsEmpty())
                throw new ArgumentException();
        }

        public void Initialize()
        {
            if (mass != 0)
                throw new InvalidOperationException($"{nameof(NonBasicRes)} is alrealy initialized, so can't initialize it a second time");
            mass = 0;
            ResAmounts curBasicIngredients = ResAmounts.Empty;
            foreach (var otherResInd in ResInd.All)
                if (ingredients[otherResInd] != 0)
                {
                    mass += ingredients[otherResInd] * CurResConfig.resources[otherResInd].Mass;
                    curBasicIngredients += ingredients[otherResInd] * CurResConfig.resources[otherResInd].BasicIngredients;
                }
            basicIngredients = curBasicIngredients;

            recipe = ResRecipe.Create
            (
                ingredients: ingredients,
                results: new ResAmounts
                (
                    resAmount: new(resInd: resInd, amount: 1)
                )
            )!.Value;
        }

        ResInd IResource.ResInd
            => resInd;

        ulong IResource.Mass
            => Mass;
    }
}
