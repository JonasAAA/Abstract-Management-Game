using Game1.Collections;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class MaterialPalette
    {
        // This is a method so that each of these is independent.
        // Otherwise, if want to show it on screen twice, both of those would show up in the same position, since they are the same object.
        public static IHUDElement CreateEmptyProdStatsInfluenceVisual()
            => ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
            (
                neededElectricityGraph: ResAndIndustryUIAlgos.emptyProdNeededElectricityFunctionGraph,
                throughputGraph: ResAndIndustryUIAlgos.emptyProdThroughputFunctionGraph,
                nodeState: null
            );

        public static Result<MaterialPalette, TextErrors> CreateAndAddToResConfig(string name, Color color, ProductClass productClass, EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices)
        {
            productClass.ThrowIfWrongIMatPurposeSet(materialChoices: materialChoices);
            MaterialPalette materialPalette = new
            (
                name: name,
                image: ColorRect.CreateIconSized(color: color),
                smallImage: ColorRect.CreateSmallIconSized(color: color),
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
        public readonly IImage image;
        public readonly IImage smallImage;
        public readonly ProductClass productClass;
        public readonly EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices;
        public readonly ResAmounts<Material> materialAmounts;
        private readonly FunctionGraphImage<SurfaceGravity, Propor> prodNeededElectricityFunctionGraph;
        private readonly FunctionGraphImage<Temperature, Propor> prodThroughputFunctionGraph;

        public MaterialPalette(string name, IImage image, IImage smallImage, ProductClass productClass, EfficientReadOnlyDictionary<MaterialPurpose, Material> materialChoices, ResAmounts<Material> materialAmounts)
        {
            this.name = name;
            this.image = image;
            this.smallImage = smallImage;
            this.productClass = productClass;
            this.materialChoices = materialChoices;
            this.materialAmounts = materialAmounts;
            prodNeededElectricityFunctionGraph = ResAndIndustryUIAlgos.CreateGravityFunctionGraph
            (
                func: gravity => ResAndIndustryAlgos.NeededElectricity(materialPalette: this, gravity: gravity)
            );
            prodThroughputFunctionGraph = ResAndIndustryUIAlgos.CreateTemperatureFunctionGraph
            (
                func: temper => ResAndIndustryAlgos.Throughput(materialPalette: this, temperature: temper)
            );
        }

        // This is a method so that each prod stats is independent.
        // Otherwise, if want to show it on screen twice, both of those would show up in the same position, since they are the same object.
        public IHUDElement CreateProdStatsInfluenceVisual()
            => ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
            (
                neededElectricityGraph: prodNeededElectricityFunctionGraph,
                throughputGraph: prodThroughputFunctionGraph,
                nodeState: null
            );

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
