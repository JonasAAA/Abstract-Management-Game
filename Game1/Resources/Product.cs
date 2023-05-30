using Game1.Collections;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Product : IResource
    {
        [Serializable]
        public sealed class Params
        {
            public EfficientReadOnlyDictionary<IMaterialPurpose, Area> MaterialTargetAreas
                => ingredients.materialTargetAreas;

            public readonly GeneralProdAndMatAmounts ingredients;
            public readonly Area targetArea;
            public readonly MechComplexity complexity;
            //private readonly HashSet<IMaterialPurpose> neededPurposes;
            //private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> materialPropors;
            //private readonly Area targetArea;

            public Params(GeneralProdAndMatAmounts ingredients)
            {
                this.ingredients = ingredients;
                targetArea = ingredients.targetArea;
                complexity = ingredients.complexity;
                //materialTargetAreas = generalRecipe.materialTargetAreas;
                //materialPropors = generalRecipe.materialPropors;

                var neededPurposes =
                    (from matPurpAndTargetArea in ingredients.materialTargetAreas
                     where !matPurpAndTargetArea.Value.IsZero
                     select matPurpAndTargetArea.Key).ToHashSet();

                Debug.Assert
                (
                    CreateProduct(materialChoices: MaterialChoices.empty).SwitchExpression
                    (
                        ok: _ => EfficientReadOnlyHashSet<IMaterialPurpose>.empty,
                        error: errors => errors
                    ).SetEquals(neededPurposes)
                );
                if (neededPurposes.Count is 0)
                    throw new ArgumentException("Product should require at least one material to be created");
            }

            public Result<Product, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateProduct(MaterialChoices materialChoices)
            {
                MaterialChoices neededMaterialChoices = materialChoices.FilterOutUnneededMaterials(ingredients: ingredients);
                return Result.Lift
                (
                    func: (arg1, arg2) => new Product(parameters: this, materialChoices: neededMaterialChoices, productIngredients: new(arg1), materialIngredients: new(arg2)),
                    arg1: ingredients.ingredProdToAmounts.SelectMany
                    (
                        func: ingredProdToAmount => ingredProdToAmount.prodParams.CreateProduct(materialChoices: neededMaterialChoices).Select
                        (
                            func: ingredProd => new ResAmount<Product>(ingredProd, ingredProdToAmount.amount)
                        )
                    ),
                    arg2: ingredients.ingredMatPurposeToTargetAreas.SelectMany<KeyValuePair<IMaterialPurpose, Area>, ResAmount<Material>, IMaterialPurpose>
                    (
                        func: ingredMatPurposeToArea => neededMaterialChoices.GetValueOrDefault(ingredMatPurposeToArea.Key) switch
                        {
                            null => new(errors: new(value: ingredMatPurposeToArea.Key)),
                            Material material => new(ok: new ResAmount<Material>(material, amount: ResAndIndustryAlgos.GatMaterialAmountFromArea(material: material, area: ingredMatPurposeToArea.Value)))
                        }
                    )
                );
            }
        }

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        //public Area Area { get; }
        public Area TargetArea { get; }
        public RawMaterialsMix RawMatComposition { get; }
        /// <summary>
        /// If tempearature is any higher, the product is destroyed, i.e. turned into garbage.
        /// </summary>
        public Temperature DestructionPoint { get; }
        public ResRecipe Recipe { get; }

        private readonly Params parameters;
        private readonly MaterialChoices materialChoices;

        private readonly SomeResAmounts<Product> productIngredients;
        private readonly SomeResAmounts<Material> materialIngredients;

        private Product(Params parameters, MaterialChoices materialChoices, SomeResAmounts<Product> productIngredients, SomeResAmounts<Material> materialIngredients)
        {
            this.parameters = parameters;
            this.materialChoices = materialChoices;
            this.productIngredients = productIngredients;
            this.materialIngredients = materialIngredients;
            Mass = productIngredients.Mass() + materialIngredients.Mass();
            HeatCapacity = productIngredients.HeatCapacity() + materialIngredients.HeatCapacity();
            //Area = arg1.Area() + arg2.Area();
            RawMatComposition = productIngredients.RawMatComposition() + materialIngredients.RawMatComposition();
            DestructionPoint = ResAndIndustryAlgos.DestructionPoint
            (
                ingredients: parameters.ingredients,
                materialChoices: materialChoices
            );

            TargetArea = parameters.targetArea;

            // Need this before creating the recipe since to create SomeResAmounts you need all used resources to be registered first
            CurResConfig.AddRes(resource: this);

            Recipe = ResRecipe.Create
            (
                ingredients: productIngredients.Generalize() + materialIngredients.Generalize(),
                results: new(res: this, amount: 1)
            )!.Value;
        }
    }
}
