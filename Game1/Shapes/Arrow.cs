namespace Game1.Shapes
{
    [Serializable]
    public sealed class Arrow : VectorShape
    {
        private static readonly Texture2D triangleTexture;

        static Arrow()
            => triangleTexture = C.LoadTexture(name: "triangle");

        protected sealed override Texture2D Texture
            => triangleTexture;

        public Arrow(IParams parameters)
            : base(parameters: parameters)
        { }

        protected sealed override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => (UDouble)dirPropor + (UDouble)orthDirPropor < 1;
    }
}
