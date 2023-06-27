namespace Game1.Shapes
{
    [Serializable]
    public sealed class InnerRing : Ring
    {
        private const int ringTextureWidthAndHeight = 2048;
        private static readonly Texture2D ringTexture;

        static InnerRing()
        {
            UDouble outerRadius = ringTextureWidthAndHeight * (UDouble).5,
                innerRadius = GetInnerRadius(outerRadius: outerRadius);

            ringTexture = C.CreateTexture
            (
                width: ringTextureWidthAndHeight,
                height: ringTextureWidthAndHeight,
                colorFromRelToCenterPos: relToCenterPos => Propor.Create
                (
                    part: (double)outerRadius - relToCenterPos.Length(),
                    whole: (double)outerRadius - innerRadius
                ) switch
                {
                    Propor propor => Color.White * .75f * (float)propor,
                    null => Color.Transparent
                }
            );
        }

        protected override Texture2D RingTexture
            => ringTexture;

        public InnerRing(IParamsWithInnerRadius parameters)
            : base(paramsWithInnerRadius: parameters)
        { }
    }
}
