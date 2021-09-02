using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public static class ArrowDrawer
    {
        private static readonly Texture2D pixel;

        static ArrowDrawer()
            => pixel = C.Content.Load<Texture2D>("pixel");

        public static void DrawArrow(Position start, Position end, Color color)
        {
            if (start == end)
                throw new ArgumentException();

            if (start is null)
                throw new ArgumentNullException();

            if (end is null)
                throw new ArgumentNullException();

            if (pixel is null)
                throw new Exception();

            Vector2 pos1 = start.ToVector2(),
                pos2 = end.ToVector2();

            C.SpriteBatch.Draw
            (
                texture: pixel,
                position: (pos1 + pos2) / 2,
                sourceRectangle: null,
                color: color,
                rotation: C.Rotation(vector: pos1 - pos2),
                origin: new Vector2(.5f, .5f),
                scale: new Vector2(Vector2.Distance(pos1, pos2), 10),
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }
}
