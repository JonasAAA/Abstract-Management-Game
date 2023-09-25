namespace Game1.Shapes
{
    [Serializable]
    public sealed class LineSegment : VectorShape
    {
        protected sealed override Texture2D Texture
            => C.PixelTexture;

        public LineSegment(IParams parameters)
            : base(parameters: parameters)
        { }

        protected sealed override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => true;
    }
}
