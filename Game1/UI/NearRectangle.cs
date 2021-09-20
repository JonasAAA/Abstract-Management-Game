using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public abstract class NearRectangle : Shape
    {
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
        public float Width
        {
            get => width;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
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
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
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
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
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
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                minHeight = value;
                if (height < minHeight)
                    height = minHeight;
            }
        }

        public event Action WidthChanged, HeightChanged;

        private float width, height, minWidth, minHeight;

        protected NearRectangle(float width, float height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException();
            this.width = width;
            if (height < 0)
                throw new ArgumentOutOfRangeException();
            this.height = height;

            MinWidth = 0;
            MinHeight = 0;
        }

        public Vector2 GetPosition(HorizPos horizOrigin, VertPos vertOrigin)
            => new(Center.X + (int)horizOrigin * Width * .5f, Center.Y + (int)vertOrigin * Height * .5f);

        public void SetPosition(Vector2 position, HorizPos horizOrigin, VertPos vertOrigin)
            => Center = new Vector2(position.X - (int)horizOrigin * Width * .5f, position.Y - (int)vertOrigin * Height * .5f);
    }
}
