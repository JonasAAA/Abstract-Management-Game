using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidCosmicBodyInfo
    {
        public static Result<FullValidCosmicBodyInfo, TextErrors> Create(ValidCosmicBodyInfo cosmicBodyInfo)
            => cosmicBodyInfo.Composition
            .SelectMany(FullValidRawMatPropor.Create)
            .Select(ExtensionMethods.ToEfficientReadOnlyCollection)
            .SelectMany<FullValidCosmicBodyInfo>
            (
                func: composition =>
                {
                    if (composition.Count is 0)
                        return new(errors: new($"Cosmic body {nameof(cosmicBodyInfo.Composition)} must not be empty"));
                    var percentageSum = composition.Sum(rawMatPropor => rawMatPropor.Percentage);
                    if (percentageSum != 100)
                    {
                        ValidRawMatPropor firstRawMatPropor = cosmicBodyInfo.Composition.First();
                        return new(errors: new($"Cosmic body {nameof(cosmicBodyInfo.Composition)} must have the sum of {nameof(firstRawMatPropor.Percentage)} be exactlty 100.\nCosmic body {cosmicBodyInfo.Name} has the sum {percentageSum} instead"));
                    }
                    return new
                    (
                        ok: new FullValidCosmicBodyInfo
                        (
                            name: cosmicBodyInfo.Name,
                            position: cosmicBodyInfo.Position,
                            radius: cosmicBodyInfo.Radius,
                            composition: composition
                        )
                    );
                }
            );

        public string Name { get; }
        public MyVector2 Position { get; }
        public Length Radius { get; }
        public EfficientReadOnlyCollection<FullValidRawMatPropor> Composition { get; }

        private FullValidCosmicBodyInfo(string name, MyVector2 position, Length radius, EfficientReadOnlyCollection<FullValidRawMatPropor> composition)
        {
            Name = name;
            Position = position;
            Radius = radius;
            Composition = composition;
        }
    }
}
