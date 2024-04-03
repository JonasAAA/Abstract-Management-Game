using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidCosmicBodyInfo
    {
        public static ValidCosmicBodyInfo CreateOrThrow(CosmicBodyInfo cosmicBodyInfo)
        {
            if (cosmicBodyInfo.Radius <= 0)
                throw new ContentException($"Cosmic body {nameof(cosmicBodyInfo.Radius)} must be positive");
            var composition = cosmicBodyInfo.Composition.Select(ValidRawMatPropor.CreateOrThrow).ToEfficientReadOnlyCollection();
            if (composition.Select(rawMatPropor => rawMatPropor.RawMaterial).ContainsDuplicates())
                throw new ContentException($"Cosmic body {nameof(cosmicBodyInfo.Composition)} must specify each raw material at most once");
            return CreateOrThrow
            (
                name: cosmicBodyInfo.Name,
                position: new(x: SignedLength.CreateFromM(cosmicBodyInfo.Position.X), y: SignedLength.CreateFromM(cosmicBodyInfo.Position.Y)),
                radius: Length.CreateFromM((UDouble)cosmicBodyInfo.Radius),
                composition: composition
            );
        }

        public static ValidCosmicBodyInfo CreateOrThrow(string name, MyVector2 position, Length radius, EfficientReadOnlyCollection<ValidRawMatPropor> composition)
        {
            if (name.Length == 0)
                throw new ContentException(message: $"Cosmic body name can't be empty");
            if (composition.ContainsDuplicates())
                throw new ContentException(message: "Cosmic body composition must not contain duplicate raw materials");
            return new
            (
                name: name,
                position: position,
                radius: radius,
                composition: composition
            );
        }

        public string Name { get; }
        public MyVector2 Position { get; }
        public Length Radius { get; }
        public EfficientReadOnlyCollection<ValidRawMatPropor> Composition { get; }

        private ValidCosmicBodyInfo(string name, MyVector2 position, Length radius, EfficientReadOnlyCollection<ValidRawMatPropor> composition)
        {
            Name = name;
            Position = position;
            Radius = radius;
            Composition = composition;
        }

        public CosmicBodyInfo ToJsonable()
            => new()
            {
                Name = Name,
                Position = new()
                {
                    X = Position.X.valueInM,
                    Y = Position.Y.valueInM
                },
                Radius = Radius.valueInM,
                Composition = Composition.Select(rawMatPropor => rawMatPropor.ToJsonable()).ToArray()
            };
    }
}
