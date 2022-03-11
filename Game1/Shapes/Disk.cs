using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.Shapes
{
    [Serializable]
    public class Disk : Shape
    {
        private static readonly Texture2D diskTexture;

        static Disk()
            => diskTexture = C.LoadTexture(name: "big disk");

        public readonly IReadOnlyChangingFloat radius;

        public Disk(IReadOnlyChangingFloat radius)
            => this.radius = radius;

        public override bool Contains(Vector2 position)
            => Vector2.Distance(position, Center) < radius.Value;

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new Vector2(diskTexture.Width, diskTexture.Height) * .5f,
                scale: 2 * radius.Value / diskTexture.Width
            );
    }
}
