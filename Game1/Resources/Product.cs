using System.Collections.Immutable;
using Game1.Collections;
﻿using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class Product : IResource
    {
        [Serializable]
        public sealed class Params
        {
            private readonly List<(Params prodParams, ulong amount)> ingredProdToAmounts;
            private readonly List<(MaterialPurpose purpose, ulong area)> ingredMatPurposeToAreas;

            public Params(List<(Params prodParams, ulong amount)> ingredProdToAmounts, List<(MaterialPurpose purpose, ulong area)> ingredMatPurposeToAreas)
            {
                this.ingredProdToAmounts = ingredProdToAmounts;
                this.ingredMatPurposeToAreas = ingredMatPurposeToAreas;
            }

            public Result<Product, IEnumerable<MaterialPurpose>> CreateProduct(Dictionary<MaterialPurpose, Material> materialChoices)
                => Result.CallFunc
                (
                    func: (productIngredients, materialIngredients) => new Product(parameters: this, materialChoices: materialChoices, productIngredients, materialIngredients),
                    arg1: ingredProdToAmounts.FlatMap
                    (
                        func: ingredProdToAmount => ingredProdToAmount.prodParams.CreateProduct(materialChoices: materialChoices).Map
                        (
                            func: ingredProd => new ResAmount<Product>(ingredProd, ingredProdToAmount.amount)
                        )
                    ).Map(prodAndAmounts => new SomeResAmounts<Product>(prodAndAmounts)),
                    arg2: ingredMatPurposeToAreas.FlatMap<(MaterialPurpose purpose, ulong area), ResAmount<Material>, MaterialPurpose>
                    (
                        func: ingredMatPurposeToArea => materialChoices.GetValueOrDefault(ingredMatPurposeToArea.purpose) switch
                        {
                            null => new(errors: new[] { ingredMatPurposeToArea.purpose }),
                            Material material => new(ok: new ResAmount<Material>(material, amount: material.GetAmountFromArea(area: ingredMatPurposeToArea.area)))
                        }
                    ).Map(matAndAmounts => new SomeResAmounts<Material>(matAndAmounts))
                );
        }

        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public SomeResAmounts<RawMaterial> RawMatComposition { get; }

        private readonly Params parameters;
        private readonly Dictionary<MaterialPurpose, Material> materialChoices;

        private readonly SomeResAmounts<Product> productIngredients;
        private readonly SomeResAmounts<Material> materialIngredients;

        private Product(Params parameters, Dictionary<MaterialPurpose, Material> materialChoices, SomeResAmounts<Product> productIngredients, SomeResAmounts<Material> materialIngredients)
        {
            this.parameters = parameters;
            this.materialChoices = materialChoices;
            this.productIngredients = productIngredients;
            this.materialIngredients = materialIngredients;
            Mass = productIngredients.Mass() + materialIngredients.Mass();
            HeatCapacity = productIngredients.HeatCapacity() + materialIngredients.HeatCapacity();
            Area = productIngredients.Area() + materialIngredients.Area();
            RawMatComposition = productIngredients.RawMatComposition() + materialIngredients.RawMatComposition();

            CurResConfig.AddRes(resource: this);
        }
    }
}
