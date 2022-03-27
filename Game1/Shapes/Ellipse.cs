namespace Game1.Shapes
{
    [Serializable]
    public class Ellipse : NearRectangle
    {
        private static readonly Texture2D diskTexture;

        static Ellipse()
            => diskTexture = C.LoadTexture(name: "big disk");

        public Ellipse(float width, float height)
            : base(width: width, height: height)
        { }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            float propX = 2 * relPos.X / Width,
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
                origin: new Vector2(diskTexture.Width, diskTexture.Height) * .5f,
                scale: new Vector2(Width / diskTexture.Width, Height / diskTexture.Height)
            );
    }
}
