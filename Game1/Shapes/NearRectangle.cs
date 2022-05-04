using Game1.Delegates;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class NearRectangle : Shape
    {
        public abstract class Factory
        {
            public abstract NearRectangle CreateNearRectangle(Shape.IState parameters);
        }

        public abstract record ImmutableParams(UDouble InitWidth, UDouble InitHeight, Color Color) : IState
        { }

        public new interface IState : Shape.IState
        {
            public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }
            public MyVector2 Center { get; set; }
            public UDouble Width { get; set; }
            public UDouble Height { get; set; }
            public UDouble MinWidth { get; set; }
            public UDouble MaxWidth { get; set; }
        }

        public Event<ISizeOrPosChangedListener> SizeOrPosChanged { get; }

        public MyVector2 Center
        {
            get => state.Center;
            set
            {
                if (!MyMathHelper.IsTiny(value: MyVector2.Distance(state.Center, value)))
                {
                    state.Center = value;
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
        public UDouble Width
        {
            get => state.Width;
            set
            {
                value = MyMathHelper.Max(value, state.MinWidth);
                if (!MyMathHelper.AreClose(state.Width, value))
                {
                    state.Width = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }
        public virtual UDouble Height
        {
            get => state.Height;
            set
            {
                value = MyMathHelper.Max(value, state.MinHeight);
                if (!MyMathHelper.AreClose(state.Height, value))
                {
                    state.Height = value;
                    RaiseSizeOrPosChanged();
                }
            }
        }
        public UDouble MinWidth
        {
            get => state.MinWidth;
            set
            {
                state.MinWidth = value;
                if (Width < state.MinWidth)
                    Width = state.MinWidth;
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

        private readonly IState state;

        //private MyVector2 center;
        //private UDouble width, height, minWidth, minHeight;

        protected NearRectangle(IState state)
            : base(state: state)
        {
            this.state = state;
            //width = parameters.InitWidth;
            //height = parameters.InitHeight;

            //minWidth = 0;
            //minHeight = 0;

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
