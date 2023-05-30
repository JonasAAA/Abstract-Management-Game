using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class MyRectangle : NearRectangle
    {
        public MyRectangle()
            : this(width: 2 * ActiveUIManager.RectOutlineWidth, height: 2 * ActiveUIManager.RectOutlineWidth)
        { }

        public MyRectangle(UDouble width, UDouble height)
            : base(width: width, height: height)
        {
            MinWidth = 2 * ActiveUIManager.RectOutlineWidth;
            MinHeight = 2 * ActiveUIManager.RectOutlineWidth;
        }

        public override bool Contains(MyVector2 position)
        {
            MyVector2 relPos = position - Center;
            return MyMathHelper.Abs(relPos.X) < Width * (UDouble).5 && MyMathHelper.Abs(relPos.Y) < Height * (UDouble).5;
        }

        public override void Draw(Color color)
        {
            if (Width.IsCloseTo(other: 0) || Height.IsCloseTo(other: 0))
                return;

            C.Draw
            (
                texture: C.PixelTexture,
                position: TopLeftCorner,
                color: color,
                rotation: 0,
                origin: MyVector2.zero,
                scaleX: Width,
                scaleY: Height
            );

            Color outlineColor = Color.Black;

            DrawOutline
            (
                Start: TopLeftCorner,
                End: TopRightCorner,
                Color: outlineColor
            );

            DrawOutline
            (
                Start: TopRightCorner,
                End: BottomRightCorner,
                Color: outlineColor
            );

            DrawOutline
            (
                Start: BottomRightCorner,
                End: BottomLeftCorner,
                Color: outlineColor
            );

            DrawOutline
            (
                Start: BottomLeftCorner,
                End: TopLeftCorner,
                Color: outlineColor
            );
        }

        /// <param Name="toLeft">is start top, end is bottom</param>
        private static void DrawOutline(MyVector2 Start, MyVector2 End, Color Color, bool toLeft = false)
        {
            MyVector2 direction = MyVector2.Normalized(End - Start);
            MyVector2 origin = toLeft switch
            {
                true => new MyVector2(.5, 1),
                false => new MyVector2(.5, 0)
            };
            C.Draw
            (
                texture: C.PixelTexture,
                position: (Start + End) / 2,
                color: Color,
                rotation: MyMathHelper.Rotation(vector: direction),
                origin: origin,
                scaleX: MyVector2.Distance(Start, End),
                scaleY: ActiveUIManager.RectOutlineWidth
            );
        }
    }
}
