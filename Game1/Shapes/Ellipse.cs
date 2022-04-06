namespace Game1.Shapes
{
    [Serializable]
    public class Ellipse : NearRectangle
    {
        private static readonly Texture2D diskTexture;

        static Ellipse()
            => diskTexture = C.LoadTexture(name: "big disk");

        public Ellipse(UDouble width, UDouble height)
            : base(width: width, height: height)
        { }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - Center;
            double propX = 2 * relPos.X / (double)Width,
                propY = 2 * relPos.Y / (double)Height;
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
