namespace Game1.Shapes
{
    [Serializable]
    public class Image : BaseImage
    {
        public Image(string imageName, MyVector2? origin = null, UDouble? width = null, UDouble? height = null)
            : base(texture: MyTexture.GetTexture(textureName: imageName), origin: origin, width: width, height: height)
        { }
    }
}
