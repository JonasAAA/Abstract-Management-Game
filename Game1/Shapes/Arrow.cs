using Game1.ChangingValues;

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

        public Arrow(MyVector2 startPos, MyVector2 endPos, IReadOnlyChangingUDouble baseWidth)
            : base(startPos: startPos, endPos: endPos, width: baseWidth)
        { }

        protected override bool Contains(Propor dirPropor, Propor orthDirPropor)
            => (UDouble)dirPropor + (UDouble)orthDirPropor < 1;
    }
}
