using Microsoft.Xna.Framework;
using System;

namespace Game1.Shapes
{
    [Serializable]
    public class CustomImage : BaseImage
    {
        private readonly new CustomTexture texture;

        public CustomImage(string imageName, Vector2? origin = null, float? width = null, float? height = null)
            : base(texture: new CustomTexture(textureName: imageName), origin: origin, width: width, height: height)
        {
            texture = (CustomTexture)base.texture;
        }

        public void StartEdit()
            => texture.StartEdit();

        public void DrawLineInImage(Vector2 worldPos1, Vector2 worldPos2, Color color)
            => texture.DrawLineInTexture
            (
                pos1: TexturePos(position: worldPos1).ToPoint(),
                pos2: TexturePos(position: worldPos2).ToPoint(),
                color: color
            );

        public void EndEdit()
            => texture.EndEdit();
    }
}
