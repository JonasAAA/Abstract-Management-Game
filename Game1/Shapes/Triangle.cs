namespace Game1.Shapes
{
    [Serializable]
    public class Triangle : NearRectangle
    {
        public new interface IParams : NearRectangle.IState
        {
            public Direction InitDirection { get; }
        }

        public enum Direction
        {
            Up = -1,
            Left = 2,
            Down = 1,
            Right = 0
        }

        private static readonly Texture2D triangleTexture;
        static Triangle()
            => triangleTexture = C.LoadTexture(name: "triangle");

        private MyVector2 BasePos
            => Center - dirVector * mainAltitudeLength * .5;

        private readonly double rotation;
        private readonly MyVector2 origin, dirVector, orthDir;
        private readonly UDouble baseLength, mainAltitudeLength, scaleX, scaleY;

        public Triangle(IParams parameters)
            : base(state: parameters)
        {
            int direction = (int)parameters.InitDirection;
            rotation = direction * MyMathHelper.pi / 2;
            origin = new MyVector2(triangleTexture.Width, triangleTexture.Height) * .5;
            dirVector = MyMathHelper.Direction(rotation: rotation);
            orthDir = new MyVector2(-dirVector.Y, dirVector.X);
            baseLength = (direction % 2) switch
            {
                0 => Height,
                not 0 => Width
            };
            mainAltitudeLength = (direction % 2) switch
            {
                0 => Width,
                not 0 => Height
            };
            scaleX = mainAltitudeLength / (UDouble)triangleTexture.Height;
            scaleY = baseLength / (UDouble)triangleTexture.Width;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - BasePos;
            double dirProp = MyVector2.Dot(relPos, dirVector) / mainAltitudeLength,
                orthDirProp = MyMathHelper.Abs(MyVector2.Dot(relPos, orthDir) / (baseLength * .5));
            if (dirProp is < 0 or >= 1 || orthDirProp >= 1)
                return false;
            return dirProp + orthDirProp < 1;
        }

        protected override void Draw(Color color)
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
