//namespace Game1.Resources
//{
//    [Serializable]
//    public sealed class NonBasicRes : IResource
//    {
//        public string Name { get; }
//        public Mass Mass { get; private set; }
//        public HeatCapacity HeatCapacity { get; private set; }
//        public ulong Area { get; private set; }
//        public Propor Reflectance { get; private set; }
//        public Propor Emissivity { get; private set; }
//        public ResAmounts BasicIngredients { get; private set; }
//        public ResRecipe Recipe { get; private set; }

//        public readonly NonBasicResInd resInd;
//        public readonly ResAmounts ingredients;

//        public NonBasicRes(string Name, NonBasicResInd resInd, ResAmounts ingredients)
//        {
//            Name = Name;
//            this.resInd = resInd;
//            this.ingredients = ingredients;
//            Mass = Mass.zero;
//            foreach (var otherResInd in ResInd.All)
//                if (otherResInd >= resInd && ingredients[otherResInd] > 0)
//                    throw new ArgumentException();
//            if (ingredients.IsEmpty())
//                throw new ArgumentException();
//        }

//        public void Initialize()
//        {
//            if (!Mass.IsZero)
//                throw new InvalidOperationException($"{nameof(NonBasicRes)} is alrealy initialized, so can't initialize it a second time");

//            Mass = ingredients.Mass();
//            HeatCapacity = ingredients.HeatCapacity();
//            Area = ingredients.Area();
//            Reflectance = ingredients.Reflectance();
//            Emissivity = ingredients.Emissivity();
//            BasicIngredients = ingredients.ConvertToBasic();
//            Recipe = ResRecipe.Create
//            (
//                ingredients: ingredients,
//                results: new(resInd: resInd, amount: 1)
//            )!.Value;
//        }

//        ResInd IResource.ResInd
//            => resInd;
//    }
//}
