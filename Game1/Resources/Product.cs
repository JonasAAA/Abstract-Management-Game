using Game1.Collections;
using Game1.UI;
using System.Resources;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Product : IResource
    {
        [Serializable]
        public sealed class Params
        {
            private static readonly Dictionary<ProductClass, ulong> nextInds = ProductClass.all.ToDictionary(elementSelector: prodClass => 0ul);

            private static ulong GetNextInd(ProductClass productClass)
            {
                var ind = nextInds[productClass];
                if (ind > ResAndIndustryAlgos.maxRawMatInd)
                    throw new ArgumentException();
                nextInds[productClass]++;
                return ind;
            }

            public static Params CreateNextOrThrow(string name, ProductClass productClass, ulong materialPaletteAmount, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
            {
                ulong indInClass = GetNextInd(productClass: productClass);
                foreach (var (ingredProd, amount) in ingredProdToAmounts)
                {
                    if (amount is 0)
                        throw new ArgumentException();
                    if (ingredProd.productClass != productClass)
                        throw new ArgumentException();
                    if (ingredProd.indInClass >= indInClass)
                        throw new ArgumentException();
                }
                return new
                (
                    name: name,
                    productClass: productClass,
                    materialPaletteAmount: materialPaletteAmount,
                    indInClass: indInClass,
                    ingredProdToAmounts: ingredProdToAmounts
                );
            }

            public readonly string name;
            public readonly ProductClass productClass;
            public readonly ulong materialPaletteAmount, indInClass;
            public readonly EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts;
            public readonly AreaInt area, recipeArea;
            public readonly MechComplexity complexity;

            //private readonly HashSet<IMaterialPurpose> neededPurposes;
            //private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;

            private Params(string name, ProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
            {
                ulong ingredientAmount = productClass.matPurposeToAmount.Values.Sum() * materialPaletteAmount + ingredProdToAmounts.Sum(prodParamsAndAmount => prodParamsAndAmount.amount);
                Debug.Assert
                (
                    ResAndIndustryAlgos.productRecipeInputAmountMultiple
                    // ingredient amount
                    % ingredientAmount
                    is 0
                );
                // Product will still need to know what product class it is, so probably need to take such parameter here as well.
                // That means need to assert that the components are from the same product class.
                this.name = name;
                this.productClass = productClass;
                this.materialPaletteAmount = materialPaletteAmount;
                this.indInClass = indInClass;
                this.ingredProdToAmounts = ingredProdToAmounts;
                Debug.Assert(ingredProdToAmounts.All(prodParamsAndAmounts => prodParamsAndAmounts.prodParams.productClass == productClass));
                Debug.Assert(ingredProdToAmounts.All(prodParamsAndAmounts => prodParamsAndAmounts.prodParams.indInClass < indInClass));
                area = ResAndIndustryAlgos.blockArea;
                recipeArea = area * ingredientAmount;
                complexity = ResAndIndustryAlgos.ProductMechComplexity
                (
                    productClass: productClass,
                    materialPaletteAmount: materialPaletteAmount,
                    indInClass: indInClass,
                    ingredProdToAmounts: ingredProdToAmounts
                );
            }

            /// <summary>
            /// Gets such product if it already exists, otherwise creates it.
            /// </summary>
            public Product GetProduct(MaterialPalette materialPalette)
            {
                if (materialPalette.productClass != productClass)
                    throw new ArgumentException();
                // This is needed for when this is called to get the needed material purposes prior to CurResConfig initialization
                if (CurResConfig is not null)
                    foreach (var otherProd in CurResConfig.GetCurRes<Product>())
                        if (otherProd.parameters == this && otherProd.MaterialPalette == materialPalette)
                            return otherProd;
                return new Product
                (
                    name: UIAlgorithms.ProductName(prodParamsName: name, paletteName: materialPalette.name),
                    parameters: this,
                    materialPalette: materialPalette,
                    productIngredients: new ResAmounts<Product>
                    (
                        resAmounts: ingredProdToAmounts.Select
                        (
                            ingredProdToAmount => new ResAmount<Product>
                            (
                                res: ingredProdToAmount.prodParams.GetProduct(materialPalette: materialPalette),
                                amount: ingredProdToAmount.amount
                            )
                        )
                    ),
                    materialIngredients: materialPalette.materialAmounts * materialPaletteAmount
                );
            }
        }

#warning Complete this by moving to a separate file so that this can be configured
        public static readonly EfficientReadOnlyDictionary<string, Params> productParamsDict = CreateProductParamsDict();

        private static EfficientReadOnlyDictionary<string, Params> CreateProductParamsDict()
            => new List<Params>()
            {
                Params.CreateNextOrThrow
                (
                    name: "Gear",
                    productClass: ProductClass.mechanical,
                    materialPaletteAmount: 2,
                    ingredProdToAmounts: new()
                ),
                Params.CreateNextOrThrow
                (
                    name: "Roof Tile",
                    productClass: ProductClass.roof,
                    materialPaletteAmount: 1,
                    ingredProdToAmounts: new()
                ),
                Params.CreateNextOrThrow
                (
                    name: "Wire",
                    productClass: ProductClass.electronics,
                    materialPaletteAmount: 1,
                    ingredProdToAmounts: new()
                )
            }.ToEfficientReadOnlyDict
            (
                keySelector: productParams => productParams.name
            );

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt Area { get; }
        public RawMatAmounts RawMatComposition { get; }
        public ResRecipe Recipe { get; }
        public MaterialPalette MaterialPalette { get; }
        public ulong IndInClass { get; }
        public ProductClass ProductClass { get; }

        private readonly string name;
        private readonly Params parameters;

        private Product(string name, Params parameters, MaterialPalette materialPalette, ResAmounts<Product> productIngredients, ResAmounts<Material> materialIngredients)
        {
            this.name = name;
            this.parameters = parameters;
            MaterialPalette = materialPalette;
            IndInClass = parameters.indInClass;
            ProductClass = parameters.productClass;
            var ingredients = productIngredients.ToAll() + materialIngredients.ToAll();
            var ingredAmount = ingredients.Sum(resAmount => resAmount.amount);
            RawMatComposition = new
            (
                resAmounts: ingredients.RawMatComposition().Select
                (
                    resAmount =>
                    {
                        Debug.Assert(resAmount.amount % ingredAmount is 0);
                        return new ResAmount<RawMaterial>(res: resAmount.res, resAmount.amount / ingredAmount);
                    }
                )
            );
            Debug.Assert(RawMatComposition * ingredAmount == ingredients.RawMatComposition());
            Debug.Assert
            (
                RawMatComposition.All
                (
                    rawMatAmount => rawMatAmount.amount
                        % ResAndIndustryAlgos.ProductRawMatCompositionDivisor(prodIndInClass: parameters.indInClass)
                        is 0
                )
            );
            Mass = RawMatComposition.Mass();
            HeatCapacity = RawMatComposition.HeatCapacity();
            Area = parameters.area;
            Debug.Assert(Area == RawMatComposition.Area());

            // Need this before creating the recipe since to create SomeResAmounts you need all used resources to be registered first
            CurResConfig.AddRes(resource: this);

            Recipe = ResRecipe.CreateOrThrow
            (
                ingredients: ingredients,
                results: new AllResAmounts(res: this, amount: ingredients.Sum(resAmount => resAmount.amount))
            );
        }

        public sealed override string ToString()
            => name;
    }
}
