﻿using Game1.Collections;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Product : IResource
    {
        [Serializable]
        public sealed class Params
        {
            private static readonly Dictionary<IProductClass, ulong> nextInds = IProductClass.all.ToDictionary(elementSelector: prodClass => 0ul);

            private static ulong GetNextInd(IProductClass productClass)
            {
                var ind = nextInds[productClass];
                nextInds[productClass]++;
                return ind;
            }

            public static Params CreateNextOrThrow(string name, IProductClass productClass, ulong materialPaletteAmount, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
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
            public readonly IProductClass productClass;
            public readonly ulong materialPaletteAmount, indInClass;
            public readonly EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts;
            public readonly AreaInt usefulArea;
            public readonly MechComplexity complexity;

            //private readonly HashSet<IMaterialPurpose> neededPurposes;
            //private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;
            //private readonly Area usefulArea;

            private Params(string name, IProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
            {
                // Product will still need to know what product class it is, so probably need to take such parameter here as well.
                // That means need to assert that the components are from the same product class.
                this.name = name;
                this.productClass = productClass;
                this.materialPaletteAmount = materialPaletteAmount;
                this.indInClass = indInClass;
                this.ingredProdToAmounts = ingredProdToAmounts;
                usefulArea = ingredProdToAmounts.Sum(ingredProdAndAmount => ingredProdAndAmount.prodParams.usefulArea * ingredProdAndAmount.amount)
                    + productClass.MatPurposeToAmount.Values.Sum() * ResAndIndustryAlgos.MaterialUsefulArea;
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
                        if (otherProd.parameters == this && otherProd.materialPalette == materialPalette)
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
                    productClass: IProductClass.mechanical,
                    materialPaletteAmount: 2,
                    ingredProdToAmounts: new()
                ),
                Params.CreateNextOrThrow
                (
                    name: "Roof Tile",
                    productClass: IProductClass.roof,
                    materialPaletteAmount: 1,
                    ingredProdToAmounts: new()
                ),
                Params.CreateNextOrThrow
                (
                    name: "Wire",
                    productClass: IProductClass.electronics,
                    materialPaletteAmount: 1,
                    ingredProdToAmounts: new()
                )
            }.ToEfficientReadOnlyDict
            (
                keySelector: productParams => productParams.name
            );

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }
        public ResRecipe Recipe { get; }

        private readonly string name;
        private readonly Params parameters;
        private readonly MaterialPalette materialPalette;

        private readonly ResAmounts<Product> productIngredients;
        private readonly ResAmounts<Material> materialIngredients;

        private Product(string name, Params parameters, MaterialPalette materialPalette, ResAmounts<Product> productIngredients, ResAmounts<Material> materialIngredients)
        {
            this.name = name;
            this.parameters = parameters;
            this.materialPalette = materialPalette;
            this.productIngredients = productIngredients;
            this.materialIngredients = materialIngredients;
            Mass = productIngredients.Mass() + materialIngredients.Mass();
            HeatCapacity = productIngredients.HeatCapacity() + materialIngredients.HeatCapacity();
            RawMatComposition = productIngredients.RawMatComposition() + materialIngredients.RawMatComposition();

            UsefulArea = parameters.usefulArea;

            // Need this before creating the recipe since to create SomeResAmounts you need all used resources to be registered first
            CurResConfig.AddRes(resource: this);

            Recipe = ResRecipe.CreateOrThrow
            (
                ingredients: productIngredients.ToAll() + materialIngredients.ToAll(),
                results: new AllResAmounts(res: this, amount: 1)
            );
        }

        public sealed override string ToString()
            => name;
    }
}
