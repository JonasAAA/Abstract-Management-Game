namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidCosmicBodyInfo
    {
        public static ValidCosmicBodyInfo CreateOrThrow(CosmicBodyInfo cosmicBodyInfo)
        {
            if (cosmicBodyInfo.Radius <= 0)
                throw new ContentException($"Cosmic body {nameof(cosmicBodyInfo.Radius)} must be positive");
            return CreateOrThrow
            (
                name: cosmicBodyInfo.Name,
                position: new(x: SignedLength.CreateFromM(cosmicBodyInfo.Position.X), y: SignedLength.CreateFromM(cosmicBodyInfo.Position.Y)),
                radius: Length.CreateFromM((UDouble)cosmicBodyInfo.Radius)
            );
        }

        public static ValidCosmicBodyInfo CreateOrThrow(string name, MyVector2 position, Length radius)
        {
            if (name.Length == 0)
                throw new ContentException(message: $"Cosmic body name can't be empty");
            return new
            (
                name: name,
                position: position,
                radius: radius
            );
        }

        public string Name { get; }
        public MyVector2 Position { get; }
        public Length Radius { get; }

        private ValidCosmicBodyInfo(string name, MyVector2 position, Length radius)
        {
            Name = name;
            Position = position;
            Radius = radius;
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
                Radius = Radius.valueInM
            };
    }
}
