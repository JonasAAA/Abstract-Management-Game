namespace Game1.Shapes
{
    [Serializable]
    public class Image : BaseImage
    {
        public Image(string imageName, Vector2? origin = null, float? width = null, float? height = null)
            : base(texture: MyTexture.GetTexture(textureName: imageName), origin: origin, width: width, height: height)
        { }
    }
}
