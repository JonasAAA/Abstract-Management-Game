namespace Game1.Shapes
{
    // about texture.GetData and texture.SetData https://gamedev.stackexchange.com/questions/89567/texture2d-setdata-method-overload
    [Serializable]
    public abstract class BaseImage : Shape
    {
        public float rotation;

        protected readonly BaseTexture texture;

        private readonly Vector2 origin, scale;

        protected BaseImage(BaseTexture texture, Vector2? origin = null, float? width = null, float? height = null)
        {
            this.texture = texture;
            
            this.origin = origin ?? new(this.texture.Width * .5f, this.texture.Height * .5f);
            scale = new(1);
            if (width.HasValue)
            {
                scale.X = width.Value / this.texture.Width;
                if (!height.HasValue)
                    scale.Y = scale.X;
            }
            if (height.HasValue)
            {
                scale.Y = height.Value / this.texture.Height;
                if (!width.HasValue)
                    scale.X = scale.Y;
            }

            Color = Color.White;
        }

        public override bool Contains(Vector2 position)
        {
            Point texturePos = TexturePos(position: position).ToPoint();

            if (!texture.Contains(pos: texturePos))
                return false;
            return !texture.Color(pos: texturePos).Transparent();
        }

        protected override void Draw(Color color)
            => texture.Draw
            (
                position: Center,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale
            );

        protected Vector2 TexturePos(Vector2 position)
        {
            Matrix transform = Matrix.CreateTranslation(-Center.X, -Center.Y, 0) *
                Matrix.CreateRotationZ(-rotation) *
                Matrix.CreateScale(1 / scale.X, 1 / scale.Y, 1) *
                Matrix.CreateTranslation(origin.X, origin.Y, 0);

            return Vector2.Transform(position, transform);
        }
    }
}
