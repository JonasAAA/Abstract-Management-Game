using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace Game1.Shapes
{
    [Serializable]
    public class MyRectangle : NearRectangle
    {
        private static class OutlineDrawer
        {
            private static readonly Texture2D pixelTexture;

            static OutlineDrawer()
                => pixelTexture = C.LoadTexture(name: "pixel");
            
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
                    scale: new Vector2(Vector2.Distance(Start, End), ActiveUIManager.RectOutlineWidth)
                );
            }
        }

        private static readonly Texture2D pixelTexture;

        static MyRectangle()
            => pixelTexture = C.LoadTexture(name: "pixel");

        public MyRectangle()
            : this(width: 2 * ActiveUIManager.RectOutlineWidth, height: 2 * ActiveUIManager.RectOutlineWidth)
        { }

        public MyRectangle(float width, float height)
            : base(width: width, height: height)
        {
            MinWidth = 2 * ActiveUIManager.RectOutlineWidth;
            MinHeight = 2 * ActiveUIManager.RectOutlineWidth;
        }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            return Math.Abs(relPos.X) < Width * .5f && Math.Abs(relPos.Y) < Height * .5f;
        }

        protected override void Draw(Color color)
        {
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
