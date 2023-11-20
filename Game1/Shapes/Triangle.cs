using Game1.ContentNames;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class Triangle : NearRectangle
    {
        public enum Direction
        {
            Up = -1,
            Left = 2,
            Down = 1,
            Right = 0
        }

        private static readonly Texture2D triangleTexture = C.LoadTexture(name: TextureName.triangle);

        private Vector2Bare BasePos
            => Center - dirVector * MainAltitudeLength * .5;
        private UDouble BaseLength
            => ((int)direction % 2) switch
            {
                0 => Height,
                not 0 => Width
            };
        private UDouble MainAltitudeLength
            => ((int)direction % 2) switch
            {
                0 => Width,
                not 0 => Height
            };

        private readonly Direction direction;
        private readonly double rotation;
        private readonly Vector2Bare origin, dirVector, orthDir;
        private readonly UDouble scaleX, scaleY;

        public Triangle(UDouble width, UDouble height, Direction direction)
            : base(width: width, height: height)
        {
            this.direction = direction;
            rotation = (int)direction * MyMathHelper.pi / 2;
            origin = new Vector2Bare(triangleTexture.Width, triangleTexture.Height) * .5;
            dirVector = MyMathHelper.Direction(rotation: rotation);
            orthDir = new Vector2Bare(-dirVector.Y, dirVector.X);
            scaleX = MainAltitudeLength / (UDouble)triangleTexture.Height;
            scaleY = BaseLength / (UDouble)triangleTexture.Width;
        }

        public sealed override bool Contains(Vector2Bare screenPos)
        {
            Vector2Bare relPos = screenPos - BasePos;
            double dirProp = Vector2Bare.Dot(relPos, dirVector) / MainAltitudeLength,
                orthDirProp = MyMathHelper.Abs(Vector2Bare.Dot(relPos, orthDir) / (BaseLength * .5));
            if (dirProp is < 0 or >= 1 || orthDirProp >= 1)
                return false;
            return dirProp + orthDirProp < 1;
        }

        public sealed override void Draw(Color color)
            => C.Draw
            (
                texture: triangleTexture,
                position: Center,
                color: color,
                rotation: rotation,
                origin: origin,
                scaleX: scaleX,
                scaleY: scaleY
            );
    }
}
