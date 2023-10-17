using Game1.Delegates;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class NearRectangle : Shape
    {
        // can do:
        //public abstract class GeneralParams
        //{
        //    public abstract void Make(double width, double height);
        //}
        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public Vector2Bare Center
        {
            get => center;
            set
            {
                if (!Vector2Bare.Distance(center, value).IsTiny())
                {
                    center = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }

        public Vector2Bare TopLeftCorner
        {
            get => GetSpecPos
            (
                origin: new(HorizPosEnum.Left, VertPosEnum.Top)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Left, VertPosEnum.Top)
            );
        }
        public Vector2Bare TopRightCorner
        {
            get => GetSpecPos
            (
                origin: new(HorizPosEnum.Right, VertPosEnum.Top)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Right, VertPosEnum.Top)
            );
        }
        public Vector2Bare BottomLeftCorner
        {
            get => GetSpecPos
            (
                origin: new(HorizPosEnum.Left, VertPosEnum.Bottom)
            );
            set => SetPosition
            (
                position: value,
                origin: new(HorizPosEnum.Left, VertPosEnum.Bottom)
            );
        }
        public Vector2Bare BottomRightCorner
        {
            get => GetSpecPos
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

        private Vector2Bare center;
        private UDouble width, height, minWidth, minHeight;

        protected NearRectangle(UDouble width, UDouble height)
        {
            this.width = width;
            this.height = height;

            minWidth = UDouble.zero;
            minHeight = UDouble.zero;

            SizeOrPosChanged = new();
        }

        public Vector2Bare GetSpecPos(PosEnums origin)
            => origin.GetPosInRect(center: Center, width: Width, height: Height);

        public void SetPosition(Vector2Bare position, PosEnums origin)
            => Center = origin.GetRectCenter(position: position, width: Width, height: Height);

        public void ClampPosition(double left, double right, double top, double bottom)
            => Center = new Vector2Bare
            (
                x: MyMathHelper.Clamp
                (
                    value: Center.X,
                    min: left + Width * UDouble.half,
                    max: right - Width * UDouble.half
                ),
                y: MyMathHelper.Clamp
                (
                    value: Center.Y,
                    min: top + Height * UDouble.half,
                    max: bottom - Height * UDouble.half
                )
            );

        private void RaiseSizeOrPosChanged()
            => SizeOrPosChanged.Raise(action: listener => listener.SizeOrPosChangedResponse(shape: this));
    }
}
