using Game1.ChangingValues;

namespace Game1.Shapes
{
    [Serializable]
    public class Disk : Shape
    {
        private static readonly Texture2D diskTexture;

        static Disk()
            => diskTexture = C.LoadTexture(name: "big disk");

        public readonly IReadOnlyChangingUDouble radius;

        public Disk(IReadOnlyChangingUDouble radius)
            => this.radius = radius;

        public override bool Contains(MyVector2 position)
            => MyVector2.Distance(position, Center) < radius.Value;

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new MyVector2(diskTexture.Width, diskTexture.Height) * .5,
                scale: 2 * radius.Value / (UDouble)diskTexture.Width
            );
    }
}
