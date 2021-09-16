using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.UI
{
    public class MyRectangle : Shape
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

        public float Width
        {
            get => width;
            set
            {
                value = Math.Max(value, minWidth);
                if (width != value)
                {
                    width = value;
                    WidthChanged?.Invoke();
                }
            }
        }
        public float Height
        {
            get => height;
            set
            {
                value = Math.Max(value, minHeight);
                if (height != value)
                {
                    height = value;
                    HeightChanged?.Invoke();
                }
            }
        }
        public float MinWidth
        {
            get => minWidth;
            set
            {
                minWidth = value;
                if (width < minWidth)
                    width = minWidth;
            }
        }
        public float MinHeight
        {
            get => minHeight;
            set
            {
                minHeight = value;
                if (height < minHeight)
                    height = minHeight;
            }
        }

        private float width, height, minWidth, minHeight;

        public Vector2 TopLeftCorner
        {
            get => GetPosition
            (
                horizOrigin: HorizPos.Left,
                vertOrigin: VertPos.Top
            );
            set => SetPosition
            (
                position: value,
                horizOrigin: HorizPos.Left,
                vertOrigin: VertPos.Top
            );
        }
        public Vector2 TopRightCorner
        {
            get => GetPosition
            (
                horizOrigin: HorizPos.Right,
                vertOrigin: VertPos.Top
            );
            set => SetPosition
            (
                position: value,
                horizOrigin: HorizPos.Right,
                vertOrigin: VertPos.Top
            );
        }
        public Vector2 BottomLeftCorner
        {
            get => GetPosition
            (
                horizOrigin: HorizPos.Left,
                vertOrigin: VertPos.Bottom
            );
            set => SetPosition
            (
                position: value,
                horizOrigin: HorizPos.Left,
                vertOrigin: VertPos.Bottom
            );
        }
        public Vector2 BottomRightCorner
        {
            get => GetPosition
            (
                horizOrigin: HorizPos.Right,
                vertOrigin: VertPos.Bottom
            );
            set => SetPosition
            (
                position: value,
                horizOrigin: HorizPos.Right,
                vertOrigin: VertPos.Bottom
            );
        }

        public event Action WidthChanged, HeightChanged;

        private readonly Texture2D pixelTexture;

        public MyRectangle()
            : this(width: 2 * outlineWidth, height: 2 * outlineWidth)
        { }

        public MyRectangle(float width, float height)
        {
            if (width < 2 * outlineWidth)
                throw new ArgumentOutOfRangeException();
            this.width = width;

            if (height < 2 * outlineWidth)
                throw new ArgumentOutOfRangeException();
            this.height = height;

            pixelTexture = C.Content.Load<Texture2D>("pixel");

            MinWidth = 0;
            MinHeight = 0;
        }

        public Vector2 GetPosition(HorizPos horizOrigin, VertPos vertOrigin)
            => new(Center.X + (int)horizOrigin * Width * .5f, Center.Y + (int)vertOrigin * Height * .5f);

        public void SetPosition(Vector2 position, HorizPos horizOrigin, VertPos vertOrigin)
            => Center = new Vector2(position.X - (int)horizOrigin * Width * .5f, position.Y - (int)vertOrigin * Height * .5f);

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            return Math.Abs(relPos.X) < Width * .5f && Math.Abs(relPos.Y) < Height * .5f;
        }

        public override void Draw()
        {
            if (Transparent)
                return;
            
            C.Draw
            (
                texture: pixelTexture,
                position: TopLeftCorner,
                color: Color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: new Vector2(Width, Height)
            );

            Color outlineColor = Color.Black;//Color.Lerp(Color.Red, Color, amount: .9f);

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
