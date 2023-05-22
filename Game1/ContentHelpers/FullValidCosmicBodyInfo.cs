using Game1.Collections;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidCosmicBodyInfo
    {
        public static Result<FullValidCosmicBodyInfo, EfficientReadOnlyHashSet<string>> Create(ValidCosmicBodyInfo cosmicBodyInfo)
            => new
            (
                ok: new
                (
                    name: cosmicBodyInfo.Name,
                    Position: cosmicBodyInfo.Position,
                    Radius: cosmicBodyInfo.Radius
                )
            );

        public string Name { get; }
        public MyVector2 Position { get; }
        public UDouble Radius { get; }

        private FullValidCosmicBodyInfo(string name, MyVector2 Position, UDouble Radius)
        {
            Name = name;
            this.Position = Position;
            this.Radius = Radius;
        }
    }
}
