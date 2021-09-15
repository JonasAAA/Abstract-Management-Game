using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.UI
{
    public class MyRectangle : Shape
    {
        public enum VertOrigin
        {
            Top = -1,
            Middle = 0,
            Bottom = 1
        }

        public enum HorizOrigin
        {
            Left = -1,
            Middle = 0,
            Right = 1
        }

        public float Width { get; private set; }
        public float Height { get; private set; }
        public Vector2 TopLeftCorner
        {
            get => GetPosition
            (
                horizOrigin: HorizOrigin.Left,
                vertOrigin: VertOrigin.Top
            );
            set => SetPosition
            (
                position: value,
                horizOrigin: HorizOrigin.Left,
                vertOrigin: VertOrigin.Top
            );
        }

        public event Action WidthChanged, HeightChanged;

        private readonly Texture2D pixelTexture;

        public MyRectangle()
            : this(width: 0, height: 0)
        { }

        public MyRectangle(float width, float height)
        {
            Width = width;
            Height = height;
            pixelTexture = C.Content.Load<Texture2D>("pixel");
        }

        public void SetWidth(float width, HorizOrigin horizOrigin)
        {
            float x = GetX(horizOrigin: horizOrigin);
            if (Width != width)
            {
                Width = width;
                WidthChanged?.Invoke();
            }
            SetX(x: x, horizOrigin: horizOrigin);
        }

        private float GetX(HorizOrigin horizOrigin)
            => Center.X + (int)horizOrigin * Width * .5f;

        private void SetX(float x, HorizOrigin horizOrigin)
            => Center = new Vector2(x - (int)horizOrigin * Width * .5f, Center.Y);

        public void SetHeight(float height, VertOrigin vertOrigin)
        {
            float y = GetY(vertOrigin: vertOrigin);
            if (Height != height)
            {
                Height = height;
                HeightChanged?.Invoke();
            }
            SetY(y: y, vertOrigin: vertOrigin);
        }

        private float GetY(VertOrigin vertOrigin)
            => Center.Y + (int)vertOrigin * Height * .5f;

        private void SetY(float y, VertOrigin vertOrigin)
            => Center = new Vector2(Center.X, y - (int)vertOrigin * Height * .5f);

        private Vector2 GetPosition(HorizOrigin horizOrigin, VertOrigin vertOrigin)
            => new(GetX(horizOrigin: horizOrigin), GetY(vertOrigin: vertOrigin));

        private Vector2 SetPosition(Vector2 position, HorizOrigin horizOrigin, VertOrigin vertOrigin)
            => Center = new Vector2(position.X - (int)horizOrigin * Width * .5f, position.Y - (int)vertOrigin * Height * .5f);

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - Center;
            return Math.Abs(relPos.X) < Width * .5f && Math.Abs(relPos.Y) < Height * .5f;
        }

        public override void Draw()
        {
            if (!Transparent)
                C.SpriteBatch.Draw
                (
                    texture: pixelTexture,
                    position: TopLeftCorner,
                    sourceRectangle: null,
                    color: Color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(Width, Height),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                );
        }
    }
}
