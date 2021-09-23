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

        public static void DrawArrow(Vector2 start, Vector2 end, Color color)
        {
            if (start == end)
                throw new ArgumentException();

            C.Draw
            (
                texture: pixel,
                position: (start + end) / 2,
                color: color,
                rotation: C.Rotation(vector: start - end),
                origin: new Vector2(.5f, .5f),
                scale: new Vector2(Vector2.Distance(start, end), 10)
            );
        }
    }
}
