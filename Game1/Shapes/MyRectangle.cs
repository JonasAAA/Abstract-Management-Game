using static Game1.GameConfig;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class MyRectangle : NearRectangle
    {
        public MyRectangle()
            : this(width: 2 * CurGameConfig.rectOutlineWidth, height: 2 * CurGameConfig.rectOutlineWidth)
        { }

        public MyRectangle(UDouble width, UDouble height)
            : base(width: width, height: height)
        {
            MinWidth = 2 * CurGameConfig.rectOutlineWidth;
            MinHeight = 2 * CurGameConfig.rectOutlineWidth;
        }

        public sealed override bool Contains(Vector2Bare screenPos)
        {
            Vector2Bare relPos = screenPos - Center;
            return relPos.X.Abs() < Width * UDouble.half && relPos.Y.Abs() < Height * UDouble.half;
        }

        public sealed override void Draw(Color color)
        {
            if (Width.IsCloseTo(other: 0) || Height.IsCloseTo(other: 0))
                return;

            C.Draw
            (
                texture: C.PixelTexture,
                position: TopLeftCorner,
                color: color,
                rotation: 0,
                origin: Vector2Bare.zero,
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
        private static void DrawOutline(Vector2Bare Start, Vector2Bare End, Color Color, bool toLeft = false)
        {
            var direction = Vector2Bare.Normalized(End - Start);
            var origin = toLeft switch
            {
                true => new Vector2Bare(.5, 1),
                false => new Vector2Bare(.5, 0)
            };
            C.Draw
            (
                texture: C.PixelTexture,
                position: (Start + End) / 2,
                color: Color,
                rotation: MyMathHelper.Rotation(vector: direction),
                origin: origin,
                scaleX: Vector2Bare.Distance(Start, End),
                scaleY: CurGameConfig.rectOutlineWidth
            );
        }
    }
}
