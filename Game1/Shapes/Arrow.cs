namespace Game1.Shapes
{
    [Serializable]
    public class Arrow : VectorShape
    {
        private static readonly Texture2D triangleTexture;

        static Arrow()
            => triangleTexture = C.LoadTexture(name: "triangle");

        protected override Texture2D Texture
            => triangleTexture;

        public Arrow(IParams parameters, Color color)
            : base(parameters: parameters, color: color)
        { }

        protected override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => (UDouble)dirPropor + (UDouble)orthDirPropor < 1;
    }
}
