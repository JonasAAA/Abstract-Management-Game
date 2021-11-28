using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace Game1.Shapes
{
    // about texture.GetData and texture.SetData https://gamedev.stackexchange.com/questions/89567/texture2d-setdata-method-overload
    [DataContract]
    public class Image : Shape
    {
        [DataMember] public float rotation;

        [DataMember] private readonly Texture2D texture;
        [DataMember] private readonly Vector2 origin, scale;
        // [x][y]
        [DataMember] private readonly Color[][] colorData;

        public Image(string imageName, Vector2? origin = null, float? width = null, float? height = null)
        {
            texture = C.LoadTexture(name: imageName);
            colorData = new Color[texture.Width][];
            Color[] colorData1D = new Color[texture.Width * texture.Height];
            texture.GetData(colorData1D);
            for (int x = 0; x < texture.Width; x++)
            {
                colorData[x] = new Color[texture.Height];
                for (int y = 0; y < texture.Height; y++)
                    colorData[x][y] = colorData1D[x + y * texture.Width];
            }
            
            this.origin = origin ?? new(texture.Width * .5f, texture.Height * .5f);
            scale = new(1);
            if (width.HasValue)
            {
                scale.X = width.Value / texture.Width;
                if (!height.HasValue)
                    scale.Y = scale.X;
            }
            if (height.HasValue)
            {
                scale.Y = height.Value / texture.Height;
                if (!width.HasValue)
                    scale.X = scale.Y;
            }

            Color = Color.White;
        }

        public override bool Contains(Vector2 position)
        {
            Point texturePos = TexturePos(position: position).ToPoint();

            if (texturePos.X < 0 || texturePos.X >= texture.Width || texturePos.Y < 0 || texturePos.Y >= texture.Height)
                return false;

            return !colorData[texturePos.X][texturePos.Y].Transparent();
        }

        private Vector2 TexturePos(Vector2 position)
        {
            Matrix transform = Matrix.CreateTranslation(-Center.X, -Center.Y, 0) *
                Matrix.CreateRotationZ(-rotation) *
                Matrix.CreateScale(1 / scale.X, 1 / scale.Y, 1) *
                Matrix.CreateTranslation(origin.X, origin.Y, 0);
            
            return Vector2.Transform(position, transform);
        }

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: texture,
                position: Center,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale
            );
    }
}
