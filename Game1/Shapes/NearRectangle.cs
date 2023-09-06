using Game1.Delegates;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class NearRectangle : Shape, IWithStandardPositions
    {
        // can do:
        //public abstract class GeneralParams
        //{
        //    public abstract void Make(double width, double height);
        //}
        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public MyVector2 Center
        {
            get => center;
            set
            {
                if (!MyMathHelper.IsTiny(value: MyVector2.Distance(center, value)))
                {
                    center = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }

        public MyVector2 TopLeftCorner
        {
            get => GetPosition
            (
                origin: new(HorizPosEnum.Left, VertPosEnum.Top)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Left, VertPosEnum.Top)
            );
        }
        public MyVector2 TopRightCorner
        {
            get => GetPosition
            (
                origin: new(HorizPosEnum.Right, VertPosEnum.Top)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Right, VertPosEnum.Top)
            );
        }
        public MyVector2 BottomLeftCorner
        {
            get => GetPosition
            (
                origin: new(HorizPosEnum.Left, VertPosEnum.Bottom)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Left, VertPosEnum.Bottom)
            );
        }
        public MyVector2 BottomRightCorner
        {
            get => GetPosition
            (
                origin: new(HorizPosEnum.Right, VertPosEnum.Bottom)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Right, VertPosEnum.Bottom)
            );
        }
        public UDouble Width
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
        public UDouble Height
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

        private MyVector2 center;
        private UDouble width, height, minWidth, minHeight;

        protected NearRectangle(UDouble width, UDouble height)
        {
            this.width = width;
            this.height = height;

            minWidth = 0;
            minHeight = 0;

            SizeOrPosChanged = new();
        }

        public MyVector2 GetPosition(PosEnums origin)
            => origin.GetPosInRect(center: Center, width: Width, height: Height);

        public void SetPosition(MyVector2 position, PosEnums origin)
            => Center = origin.GetRectCenter(position: position, width: Width, height: Height);

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
