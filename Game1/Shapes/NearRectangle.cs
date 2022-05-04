using Game1.Delegates;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class NearRectangle : Shape
    {
        // can do:
        //public abstract class Params
        //{
        //    public abstract void Make(double width, double height);
        //}
        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public override MyVector2 Center
        {
            get => base.Center;
            set
            {
                if (!MyMathHelper.IsTiny(value: MyVector2.Distance(base.Center, value)))
                {
                    base.Center = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }

        public MyVector2 TopLeftCorner
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
        public MyVector2 TopRightCorner
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
        public MyVector2 BottomLeftCorner
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
        public MyVector2 BottomRightCorner
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
        public virtual UDouble Width
        {
            get => width;
            set
            {
                value = MyMathHelper.Max(value, minWidth);
                if (!MyMathHelper.AreClose(width, value))
                {
                    width = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }
        public virtual UDouble Height
        {
            get => height;
            set
            {
                value = MyMathHelper.Max(value, minHeight);
                if (!MyMathHelper.AreClose(height, value))
                {
                    height = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }
        public UDouble MinWidth
        {
            get => minWidth;
            set
            {
                minWidth = value;
                if (Width < minWidth)
                    Width = minWidth;
            }
        }
        public UDouble MinHeight
        {
            get => minHeight;
            set
            {
                minHeight = value;
                if (Height < minHeight)
                    Height = minHeight;
            }
        }

        private UDouble width, height, minWidth, minHeight;

        protected NearRectangle(UDouble width, UDouble height)
        {
            this.width = width;
            this.height = height;

            minWidth = 0;
            minHeight = 0;

            SizeOrPosChanged = new();
        }

        public MyVector2 GetPosition(HorizPos horizOrigin, VertPos vertOrigin)
            => Center + new MyVector2((int)horizOrigin * Width, (int)vertOrigin * Height) * .5;

        public void SetPosition(MyVector2 position, HorizPos horizOrigin, VertPos vertOrigin)
            => Center = position - new MyVector2((int)horizOrigin * Width, (int)vertOrigin * Height) * .5;

        public void ClampPosition(double left, double right, double top, double bottom)
            => Center = new MyVector2
            (
                x: MyMathHelper.Clamp
                (
                    value: Center.X,
                    min: left + Width * .5,
                    max: right - Width * .5
                ),
                y: MyMathHelper.Clamp
                (
                    value: Center.Y,
                    min: top + Height * .5,
                    max: bottom - Height * .5
                )
            );

        private void RaiseSizeOrPosChanged()
            => SizeOrPosChanged.Raise(action: listener => listener.SizeOrPosChangedResponse(shape: this));
    }
}
