using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.UI
{
    public class Image : NearRectangle
    {
        public enum Rotation
        {
            None = 0,
            Right = -1,
            Left = 1,
            Deg180 = 2,
        }

        private readonly NearRectangle collider;
        private readonly Texture2D texture;
        private readonly Vector2 origin, scale;
        private readonly float rotation;

        public Image(bool isRectangle, string imageName, float? width = null, float? height = null, Rotation rotation = Rotation.None)
            : base(width: 0, height: 0)
        {
            texture = C.LoadTexture(name: imageName);
            origin = new(texture.Width * .5f, texture.Height * .5f);
            scale = new(1);
            if (width.HasValue)
            {
                scale.X = width.Value / texture.Width;
                if (!height.HasValue)
                    scale.Y = scale.X;
            }
            if (height.HasValue)
            {
                scale.Y = height.Value / texture.Height;
                if (!width.HasValue)
                    scale.X = scale.Y;
            }

            Color = Color.White;
            Width = texture.Width * scale.X;
            Height = texture.Height * scale.Y;
            this.rotation = (int)rotation * MathHelper.PiOver2;

            collider = isRectangle switch
            {
                true => new MyRectangle(width: Width, height: Height),
                false => new Ellipse(width: Width, height: Height)
            };
            collider.Center = Center;
            SizeOrPosChanged += () =>
            {
                collider.Width = Width;
                collider.Height = Height;
                collider.Center = Center;
            };
        }

        public override bool Contains(Vector2 position)
            => collider.Contains(position: position);

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: texture,
                position: Center,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale
            );
    }
}
