using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public abstract class NearRectangle : Shape
    {
        // can do:
        //public abstract class Params
        //{
        //    public abstract void Make(float width, float height);
        //}

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
                if (!C.IsTiny(value: width - value))
                {
                    width = value;
                    //SizeOrPosChanged.Raise(action: listener => listener.SizeOrPosChangedResponse());
                    RaiseSizeOrPosChanged();
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
                if (!C.IsTiny(value: height - value))
                {
                    height = value;
                    RaiseSizeOrPosChanged();
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

        [DataMember]
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
            => Center + new Vector2((int)horizOrigin * Width, (int)vertOrigin * Height) * .5f;

        public void SetPosition(Vector2 position, HorizPos horizOrigin, VertPos vertOrigin)
            => Center = position - new Vector2((int)horizOrigin * Width, (int)vertOrigin * Height) * .5f;

        public void ClampPosition(float left, float right, float top, float bottom)
            => Center = new Vector2
            (
                x: Math.Clamp
                (
                    value: Center.X,
                    min: left + Width * .5f,
                    max: right - Width * .5f
                ),
                y: Math.Clamp
                (
                    value: Center.Y,
                    min: top + Height * .5f,
                    max: bottom - Height * .5f
                )
            );
    }
}
