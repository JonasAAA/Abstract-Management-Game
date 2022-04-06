using Game1.ChangingValues;

namespace Game1.Shapes
{
    [Serializable]
    public class LineSegment : VectorShape
    {
        private static readonly Texture2D pixelTexture;

        static LineSegment()
            => pixelTexture = C.LoadTexture(name: "pixel");

        protected override Texture2D Texture
            => pixelTexture;

        public LineSegment(MyVector2 startPos, MyVector2 endPos, IReadOnlyChangingUDouble width)
            : base(startPos: startPos, endPos: endPos, width: width)
        { }

        protected override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => true;
    }
}
