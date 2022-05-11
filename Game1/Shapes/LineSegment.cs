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

        public LineSegment(IParams parameters)
            : base(parameters: parameters)
        { }

        protected override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => true;
    }
}
