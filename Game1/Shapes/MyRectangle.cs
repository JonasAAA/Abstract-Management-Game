using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public class MyRectangle : NearRectangle
    {
        public new class Factory : NearRectangle.Factory
        {
            private readonly record struct MyRectangeParams(UDouble InitWidth, UDouble InitHeight, Shape.IState Parameters) : IParams
            {
                public Color Color
                    => Parameters.Color;
            }

            private readonly UDouble initWidth, initHeight;

            public Factory(UDouble initWidth, UDouble initHeight)
            {
                this.initWidth = initWidth;
                this.initHeight = initHeight;
            }

            public override MyRectangle CreateNearRectangle(Shape.IState parameters)
                => new(parameters: new MyRectangeParams(InitWidth: initWidth, InitHeight: initHeight, Parameters: parameters));
        }

        public new record ImmutableParams(UDouble InitWidth, UDouble InitHeight, Color Color) : NearRectangle.ImmutableParams(InitWidth: InitWidth, InitHeight: InitHeight, Color: Color), IParams
        { }

        public new interface IParams : NearRectangle.IState
        { }

        private static class OutlineDrawer
        {
            private static readonly Texture2D pixelTexture;

            static OutlineDrawer()
                => pixelTexture = C.LoadTexture(name: "pixel");
            
            /// <param name="toLeft">is start top, end is bottom</param>
            public static void Draw(MyVector2 Start, MyVector2 End, Color Color, bool toLeft = false)
            {
                MyVector2 direction = MyVector2.Normalized(End - Start);
                MyVector2 origin = toLeft switch
                {
                    true => new MyVector2(.5, 1),
                    false => new MyVector2(.5, 0)
                };
                C.Draw
                (
                    texture: pixelTexture,
                    position: (Start + End) / 2,
                    color: Color,
                    rotation: MyMathHelper.Rotation(vector: direction),
                    origin: origin,
                    scaleX: MyVector2.Distance(Start, End),
                    scaleY: ActiveUIManager.RectOutlineWidth
                );
            }
        }

        private static readonly Texture2D pixelTexture;

        static MyRectangle()
            => pixelTexture = C.LoadTexture(name: "pixel");

        // TODO: delete
        //public MyRectangle(IParams parameters)
        //    : this(width: 2 * ActiveUIManager.RectOutlineWidth, height: 2 * ActiveUIManager.RectOutlineWidth, parameters: parameters)
        //{ }

        public MyRectangle(IParams parameters)
            : base(state: parameters)
        {
            MinWidth = 2 * ActiveUIManager.RectOutlineWidth;
            MinHeight = 2 * ActiveUIManager.RectOutlineWidth;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - Center;
            return MyMathHelper.Abs(relPos.X) < Width * (UDouble).5 && MyMathHelper.Abs(relPos.Y) < Height * (UDouble).5;
        }

        protected override void Draw(Color color)
        {
            C.Draw
            (
                texture: pixelTexture,
                position: TopLeftCorner,
                color: color,
                rotation: 0,
                origin: MyVector2.zero,
                scaleX: Width,
                scaleY: Height
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
