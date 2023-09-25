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

        private static readonly Texture2D triangleTexture = C.LoadTexture(name: "triangle");

        private MyVector2 BasePos
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
        private readonly MyVector2 origin, dirVector, orthDir;
        private readonly UDouble scaleX, scaleY;

        public Triangle(UDouble width, UDouble height, Direction direction)
            : base(width: width, height: height)
        {
            this.direction = direction;
            rotation = (int)direction * MyMathHelper.pi / 2;
            origin = new MyVector2(triangleTexture.Width, triangleTexture.Height) * .5;
            dirVector = MyMathHelper.Direction(rotation: rotation);
            orthDir = new MyVector2(-dirVector.Y, dirVector.X);
            scaleX = MainAltitudeLength / (UDouble)triangleTexture.Height;
            scaleY = BaseLength / (UDouble)triangleTexture.Width;
        }

        public sealed override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - BasePos;
            double dirProp = MyVector2.Dot(relPos, dirVector) / MainAltitudeLength,
                orthDirProp = MyMathHelper.Abs(MyVector2.Dot(relPos, orthDir) / (BaseLength * .5));
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
