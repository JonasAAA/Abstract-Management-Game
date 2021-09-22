using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.UI
{
    public class MyRectangle : NearRectangle
    {
        private static class OutlineDrawer
        {
            private static readonly Texture2D pixelTexture;

            static OutlineDrawer()
                => pixelTexture = C.Content.Load<Texture2D>("pixel");
            
            /// <param name="toLeft">is start top, end is bottom</param>
            public static void Draw(Vector2 Start, Vector2 End, Color Color, bool toLeft = false)
            {
                Vector2 direction = End - Start;
                direction.Normalize();
                Vector2 origin = toLeft switch
                {
                    true => new Vector2(.5f, 1),
                    false => new Vector2(.5f, 0)
                };
                C.Draw
                (
                    texture: pixelTexture,
                    position: (Start + End) / 2,
                    color: Color,
                    rotation: C.Rotation(vector: direction),
                    origin: origin,
                    scale: new Vector2(Vector2.Distance(Start, End), outlineWidth)
                );
            }
        }

        public static readonly float outlineWidth;

        static MyRectangle()
            => outlineWidth = 0;

        private readonly Texture2D pixelTexture;

        public MyRectangle()
            : this(width: 2 * outlineWidth, height: 2 * outlineWidth)
        { }

        public MyRectangle(float width, float height)
            : base(width: width, height: height)
        {
            MinWidth = 2 * outlineWidth;
            MinHeight = 2 * outlineWidth;

            pixelTexture = C.Content.Load<Texture2D>("pixel");
        }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            return Math.Abs(relPos.X) < Width * .5f && Math.Abs(relPos.Y) < Height * .5f;
        }

        protected override void Draw(Color color)
        {
            //if (Transparent)
            //    return;

            if (C.Transparent(color: color))
                return;

            C.Draw
            (
                texture: pixelTexture,
                position: TopLeftCorner,
                color: color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: new Vector2(Width, Height)
            );

            Color outlineColor = Color.Black;

            OutlineDrawer.Draw
            (
                Start: TopLeftCorner,
                End: TopRightCorner,
                Color: outlineColor
            );

            OutlineDrawer.Draw
            (
                Start: TopRightCorner,
                End: BottomRightCorner,
                Color: outlineColor
            );

            OutlineDrawer.Draw
            (
                Start: BottomRightCorner,
                End: BottomLeftCorner,
                Color: outlineColor
            );

            OutlineDrawer.Draw
            (
                Start: BottomLeftCorner,
                End: TopLeftCorner,
                Color: outlineColor
            );
        }
    }
}
