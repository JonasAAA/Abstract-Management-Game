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

        public LineSegment(Vector2 startPos, Vector2 endPos, IReadOnlyChangingUFloat width)
            : base(startPos: startPos, endPos: endPos, width: width)
        { }

        protected override bool Contains(float dirProp, float orthDirProp)
            => true;
    }
}
