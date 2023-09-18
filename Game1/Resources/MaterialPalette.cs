﻿using Game1.Collections;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class MaterialPalette
    {
        public static Result<MaterialPalette, TextErrors> CreateAndAddToResConfig(string name, IProductClass productClass, EfficientReadOnlyDictionary<IMaterialPurpose, Material> materialChoices)
        {
            productClass.ThrowIfWrongIMatPurposeSet(materialChoices: materialChoices);
            MaterialPalette materialPalette = new
            (
                name: name,
                productClass: productClass,
                materialChoices: materialChoices,
                materialAmounts: new ResAmounts<Material>
                (
                    resAmounts: productClass.MatPurposeToAmount.Select
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
        public readonly IProductClass productClass;
        public readonly EfficientReadOnlyDictionary<IMaterialPurpose, Material> materialChoices;
        public readonly ResAmounts<Material> materialAmounts;

        public MaterialPalette(string name, IProductClass productClass, EfficientReadOnlyDictionary<IMaterialPurpose, Material> materialChoices, ResAmounts<Material> materialAmounts)
        {
            this.name = name;
            this.productClass = productClass;
            this.materialChoices = materialChoices;
            this.materialAmounts = materialAmounts;
        }

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