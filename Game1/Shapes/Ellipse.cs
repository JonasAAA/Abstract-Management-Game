﻿using Game1.ContentNames;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class Ellipse : NearRectangle
    {
        private static readonly Texture2D diskTexture = C.LoadTexture(name: TextureName.disk);

        public Ellipse(UDouble width, UDouble height)
            : base(width: width, height: height)
        { }

        public sealed override bool Contains(Vector2Bare screenPos)
        {
            Vector2Bare relPos = screenPos - Center;
            double propX = 2 * relPos.X / Width,
                propY = 2 * relPos.Y / Height;
            return propX * propX + propY * propY < 1;
        }

        public sealed override void Draw(Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new Vector2Bare(diskTexture.Width, diskTexture.Height) * .5,
                scaleX: Width / (UDouble)diskTexture.Width,
                scaleY: Height / (UDouble)diskTexture.Height
            );
    }
}
