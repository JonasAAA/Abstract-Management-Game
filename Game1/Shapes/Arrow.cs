using Game1.ContentNames;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class Arrow : VectorShape
    {
        private static readonly Texture2D triangleTexture = C.LoadTexture(name: TextureName.triangle);

        protected sealed override Texture2D Texture
            => triangleTexture;

        public Arrow(IParams parameters, WorldCamera worldCamera)
            : base(parameters: parameters, worldCamera: worldCamera)
        { }

        protected sealed override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => (UDouble)dirPropor + (UDouble)orthDirPropor < 1;
    }
}
