using Game1.Collections;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Product<TProductClass> : IProduct
        where TProductClass : struct, IProductClass
    {
        [Serializable]
        public sealed class Params : IProduct.IParams
        {
            private static ulong nextInd = 0;

            private static ulong GetNextInd()
            {
                var ind = nextInd;
                nextInd++;
                return ind;
            }

            public static Params CreateNextOrThrow(string name, ulong materialPaletteAmount, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
            {
                ulong indInClass = GetNextInd();
                foreach (var (ingredProd, amount) in ingredProdToAmounts)
                {
                    if (amount is 0)
                        throw new ArgumentException();
                    if (ingredProd.indInClass >= indInClass)
                        throw new ArgumentException();
                }
                return new
                (
                    name: name,
                    materialPaletteAmount: materialPaletteAmount,
                    indInClass: indInClass,
                    ingredProdToAmounts: ingredProdToAmounts
                );
            }

            //public EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt> MaterialUsefulAreas
            //    => ingredients.materialUsefulAreas;

            public readonly string name;
            public readonly ulong materialPaletteAmount, indInClass;
            public readonly AreaInt usefulArea;
            public readonly MechComplexity complexity;

            //private readonly HashSet<IMaterialPurpose> neededPurposes;
            //private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;
            //private readonly Area usefulArea;

            private Params(string name, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Params prodParams, ulong amount)> ingredProdToAmounts)
            {
                // Product will still need to know what product class it is, so probably need to take such parameter here as well.
                // That means need to assert that the components are from the same product class.
                this.name = name;
                this.materialPaletteAmount = materialPaletteAmount;
                this.indInClass = indInClass;
                usefulArea = ingredProdToAmounts.Sum(ingredProdAndAmount => ingredProdAndAmount.prodParams.usefulArea * ingredProdAndAmount.amount)
                    + TProductClass.MatPurposeToMultipleOfMatTargetAreaDivisor.Values.Sum(amount => amount * ResAndIndustryAlgos.MaterialTargetAreaDivisor);
                throw new NotImplementedException();
                //complexity = ResAndIndustryAlgos.Complexity(ingredProdToAmounts: ingredProdToAmounts, ingredMatPurposeToUsefulAreas: ingredMatPurposeToUsefulAreas);
            }

            private string GenerateProductName()
                => Algorithms.GanerateNewName
                (
                    prefix: name,
                    usedNames: CurResConfig.GetCurRes<Product<TProductClass>>().Select(product => product.name).ToEfficientReadOnlyHashSet()
                );

            /// <summary>
            /// Gets such product if it already exists, otherwise creates it.
            /// </summary>
            public Product<TProductClass> GetProduct(MaterialPalette<TProductClass> materialPalette)
            {
                throw new NotImplementedException();
                //// This is needed for when this is called to get the needed material purposes prior to CurResConfig initialization
                //if (CurResConfig is not null)
                //    foreach (var otherProd in CurResConfig.GetCurRes<Product<TProductClass>>())
                //        if (otherProd.IsIdenticalTo(otherProdParams: this, otherMaterialChoices: materialChoices))
                //            return otherProd;

                //MaterialChoices neededMaterialChoices = materialChoices.FilterOutUnneededMaterials(neededMaterialPurposes: neededMaterialPurposes);
                //return Result.Lift
                //(
                //    func: (arg1, arg2) => new Product(name: GenerateProductName(), parameters: this, materialChoices: neededMaterialChoices, productIngredients: new(arg1), materialIngredients: new(arg2)),
                //    arg1: ingredients.ingredProdToAmounts.SelectMany
                //    (
                //        func: ingredProdToAmount => ingredProdToAmount.prodParams.GetProduct(materialChoices: neededMaterialChoices).Select
                //        (
                //            func: ingredProd => new ResAmount<Product>(ingredProd, ingredProdToAmount.amount)
                //        )
                //    ),
                //    arg2: ingredients.ingredMatPurposeToUsefulAreas.SelectMany<KeyValuePair<IMaterialPurpose, AreaInt>, ResAmount<Material>, IMaterialPurpose>
                //    (
                //        func: ingredMatPurposeToArea => neededMaterialChoices.GetValueOrDefault(ingredMatPurposeToArea.Key) switch
                //        {
                //            null => new(errors: new(value: ingredMatPurposeToArea.Key)),
                //            Material material => new(ok: new ResAmount<Material>(material, amount: ResAndIndustryAlgos.GatMaterialAmountFromArea(material: material, area: ingredMatPurposeToArea.Value)))
                //        }
                //    )
                //);
            }
        }

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public AreaInt UsefulArea { get; }
        public RawMatAmounts RawMatComposition { get; }
        public ResRecipe Recipe { get; }

        private readonly string name;
        private readonly Params parameters;
        private readonly MaterialPalette<TProductClass> materialPalette;

        private readonly ResAmounts<Product<TProductClass>> productIngredients;
        private readonly ResAmounts<Material> materialIngredients;

        private Product(string name, Params parameters, MaterialPalette<TProductClass> materialPalette, ResAmounts<Product<TProductClass>> productIngredients, ResAmounts<Material> materialIngredients)
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

        private bool IsIdenticalTo(Params otherProdParams, MaterialPalette<TProductClass> otherMaterialPalette)
        {
            if (parameters != otherProdParams)
                return false;
            throw new NotImplementedException();
            ////if (otherMaterialChoices.Count < materialChoices.Count)
            ////    return false;
            //foreach (var materialPurpose in parameters.neededMaterialPurposes)
            //    if (materialChoices[materialPurpose] != otherMaterialChoices.GetValueOrDefault(key: materialPurpose))
            //        return false;
            return true;
        }

        public sealed override string ToString()
            => name;
    }

    public interface IProduct : IResource
    {
        public interface IParams
        {
            public AreaInt usefulArea { get; }

            public IProduct GetProduct(IMaterialPalette materialPalette);
        }

#warning Complete this by moving to a separate file so that this can be configured
        public static readonly EfficientReadOnlyDictionary<string, IParams> productParamsDict = CreateProductParamsDict();

        private static EfficientReadOnlyDictionary<string, IParams> CreateProductParamsDict()
        {
            throw new NotImplementedException();
            //EfficientReadOnlyDictionary<IProductClass, EfficientReadOnlyDictionary<string, Params>> result = new()
            //{
            //    [IProductClass.mechanical] = ParamsDict
            //    (
            //        new Params
            //        (
            //            name: "Gear",
            //            ingredProdToAmounts: new(),
            //            ingredMatPurposeToUsefulAreas: new EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt>
            //            {
            //                [IMaterialPurpose.mechanical] = AreaInt.CreateFromMetSq(10),
            //            }
            //        )
            //    )
            //};


            ////new List<Params>()
            ////{
            ////    new
            ////    (
            ////        name: "Gear",
            ////        ingredients: new
            ////        (
            ////            ingredProdToAmounts: new(),
            ////            ingredMatPurposeToUsefulAreas: new EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt>
            ////            {
            ////                [IMaterialPurpose.mechanical] = AreaInt.CreateFromMetSq(10),
            ////            }
            ////        )
            ////    ),
            ////    new
            ////    (
            ////        name: "Roof Tile",
            ////        ingredients: new
            ////        (
            ////            ingredProdToAmounts: new(),
            ////            ingredMatPurposeToUsefulAreas: new EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt>
            ////            {
            ////                [IMaterialPurpose.roofSurface] = AreaInt.CreateFromMetSq(10)
            ////            }
            ////        )
            ////    ),
            ////    new
            ////    (
            ////        name: "Wire",
            ////        ingredients: new
            ////        (
            ////            ingredProdToAmounts: new(),
            ////            ingredMatPurposeToUsefulAreas: new EfficientReadOnlyDictionary<IMaterialPurpose, AreaInt>
            ////            {
            ////                [IMaterialPurpose.electricalConductor] = AreaInt.CreateFromMetSq(10),
            ////                [IMaterialPurpose.electricalInsulator] = AreaInt.CreateFromMetSq(5)
            ////            }
            ////        )
            ////    )
            ////}.ToEfficientReadOnlyDict
            ////(
            ////    keySelector: productParams => productParams.name
            ////);

            //EfficientReadOnlyDictionary<string, Params> ParamsDict(params Params[] prodParams)
            //    => prodParams.ToEfficientReadOnlyDict
            //    (
            //        keySelector: productParams => productParams.name
            //    );
        }

        public ResRecipe Recipe { get; }
    }
}
