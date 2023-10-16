using Game1.Collections;
using Game1.Industries;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class MaterialPalette
    {
        private static readonly IImage emptyProdThroughputFunctionGraph = IndustryUIAlgos.CreateTemperatureFunctionGraph(func: null);

        // This is a method so that each of these is independent.
        // Otherwise, if want to show it on screen twice, both of those would show up in the same position, since they are the same object.
        public static IHUDElement CreateEmptyProdStatsInfluenceVisual()
            => new ImageHUDElement(image: emptyProdThroughputFunctionGraph);

        public static Result<MaterialPalette, TextErrors> CreateAndAddToResConfig(string name, ProductClass productClass, EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices)
        {
            productClass.ThrowIfWrongIMatPurposeSet(materialChoices: materialChoices);
            MaterialPalette materialPalette = new
            (
                name: name,
                productClass: productClass,
                materialChoices: materialChoices,
                materialAmounts: new ResAmounts<Material>
                (
                    resAmounts: productClass.matPurposeToAmount.Select
                    (
                        matPurposeAndAmount => new ResAmount<Material>
                        (
                            res: materialChoices[matPurposeAndAmount.Key],
                            amount: matPurposeAndAmount.Value
                        )
                    )
                )
            );
            return CurResConfig.AddMaterialPalette(materialPalette).Select
            (
                func: _ => materialPalette
            );
        }

        public readonly string name;
        public readonly ProductClass productClass;
        public readonly EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices;
        public readonly ResAmounts<Material> materialAmounts;
        private readonly IImage prodThroughputFunctionGraph;

        public MaterialPalette(string name, ProductClass productClass, EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices, ResAmounts<Material> materialAmounts)
        {
            this.name = name;
            this.productClass = productClass;
            this.materialChoices = materialChoices;
            this.materialAmounts = materialAmounts;
            prodThroughputFunctionGraph = IndustryUIAlgos.CreateTemperatureFunctionGraph
            (
                func: temper => ResAndIndustryAlgos.Throughput(materialPalette: this, temperature: temper)
            );
        }

        // This is a method so that each prod stats is independent.
        // Otherwise, if want to show it on screen twice, both of those would show up in the same position, since they are the same object.
        public IHUDElement CreateProdStatsInfluenceVisual()
            => new ImageHUDElement(image: prodThroughputFunctionGraph);

        /// <summary>
        /// Returns text errors if contents are the same
        /// </summary>
        public Result<UnitType, TextErrors> VerifyThatHasdifferentContents(MaterialPalette otherMatPalette)
        {
            if (otherMatPalette.materialChoices.DictEquals(materialChoices))
                return new(errors: new(UIAlgorithms.ExactSamePaletteAlreadyExists));
            return new(ok: new());
        }

        public sealed override string ToString()
            => name;
    }
}
