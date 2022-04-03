using Game1.PrimitiveTypeWrappers;

namespace Game1.Shapes
{
    [Serializable]
    public class Image : BaseImage
    {
        public Image(string imageName, Vector2? origin = null, UFloat? width = null, UFloat? height = null)
            : base(texture: MyTexture.GetTexture(textureName: imageName), origin: origin, width: width, height: height)
        { }
    }
}
