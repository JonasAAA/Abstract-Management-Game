using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct MaterialPaletteChoices
    {
        public static MaterialPaletteChoices Create(List<MaterialPalette> choices)
            => new(choices: choices.ToEfficientReadOnlyDict(keySelector: materialPalette => materialPalette.productClass));

        public static MaterialPaletteChoices CreateOrThrow(EfficientReadOnlyDictionary<IProductClass, MaterialPalette> choices)
        {
            if (choices.Any(prodClassAndMatPalette => prodClassAndMatPalette.Key != prodClassAndMatPalette.Value.productClass))
                throw new ArgumentException();
            return new MaterialPaletteChoices(choices: choices);
        }

        public EfficientReadOnlyDictionary<IProductClass, MaterialPalette> Choices { get; }

        private MaterialPaletteChoices(EfficientReadOnlyDictionary<IProductClass, MaterialPalette> choices)
            => Choices = choices;

        public MaterialPalette this[IProductClass productClass]
            => Choices[productClass];

        public MaterialPaletteChoices FilterOutUnneededMatPalettes(EfficientReadOnlyHashSet<IProductClass> neededProductClasses)
            => new
            (
                choices: Choices.Where(matChoice => neededProductClasses.Contains(matChoice.Key)).ToEfficientReadOnlyDict
                (
                    keySelector: matChoice => matChoice.Key,
                    elementSelector: matChoice => matChoice.Value
                )
            );
    }
}
