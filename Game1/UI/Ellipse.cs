using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.UI
{
    public class Ellipse : NearRectangle
    {
        private readonly Texture2D diskTexture;

        public Ellipse(float width, float height)
            : base(width: width, height: height)
        {
            diskTexture = C.Content.Load<Texture2D>("big disk");
        }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            float propX = 2 * relPos.X / Width,
                propY = 2 * relPos.Y / Height;
            return propX * propX + propY * propY < 1;
        }

        protected override void Draw(Color color)
        {
            if (C.Transparent(color: color))
                return;
            C.Draw
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
}
