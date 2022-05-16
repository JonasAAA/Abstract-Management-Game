namespace Game1.Shapes
{
    [Serializable]
    public sealed class OuterRing : Ring
    {
        private const int ringTextureWidthAndHeight = 2048;
        private static readonly Texture2D ringTexture;

        static OuterRing()
        {
            UDouble outerRadius = ringTextureWidthAndHeight * (UDouble).5,
                innerRadius = GetInnerRadius(outerRadius: outerRadius);

            ringTexture = C.CreateTexture
            (
                width: ringTextureWidthAndHeight,
                height: ringTextureWidthAndHeight,
                colorFromRelToCenterPos: relToCenterPos => Propor.Create
                (
                    part: relToCenterPos.Length() - innerRadius,
                    whole: outerRadius - innerRadius
                ) switch
                {
                    Propor propor => Color.White * .5f * (float)propor,
                    null => Color.Transparent
                }
            );
        }

        protected override Texture2D RingTexture
           => ringTexture;

        public OuterRing(IParamsWithOuterRadius parameters)
            : base(paramsWithOuterRadius: parameters)
        { }
    }
}
