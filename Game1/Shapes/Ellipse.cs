namespace Game1.Shapes
{
    [Serializable]
    public class Ellipse : NearRectangle
    {
        public new record ImmutableParams(UDouble InitWidth, UDouble InitHeight, Color Color) : NearRectangle.ImmutableParams(InitWidth: InitWidth, InitHeight: InitHeight, Color: Color), IParams
        { }

        public new interface IParams : NearRectangle.IState
        { }

        private static readonly Texture2D diskTexture;

        static Ellipse()
            => diskTexture = C.LoadTexture(name: "big disk");

        public Ellipse(IParams parameters)
            : base(state: parameters)
        { }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - Center;
            double propX = 2 * relPos.X / Width,
                propY = 2 * relPos.Y / Height;
            return propX * propX + propY * propY < 1;
        }

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new MyVector2(diskTexture.Width, diskTexture.Height) * .5,
                scaleX: Width / (UDouble)diskTexture.Width,
                scaleY: Height / (UDouble)diskTexture.Height
            );
    }
}
