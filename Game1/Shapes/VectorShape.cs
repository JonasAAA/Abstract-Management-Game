using Game1.ChangingValues;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class VectorShape : Shape
    {
        public readonly Vector2 startPos, endPos;

        protected abstract Texture2D Texture { get; }
        protected readonly IReadOnlyChangingUFloat width;

        protected VectorShape(Vector2 startPos, Vector2 endPos, IReadOnlyChangingUFloat width)
        {
            if (C.IsTiny(Vector2.Distance(startPos, endPos)))
                throw new ArgumentException();
            this.startPos = startPos;
            this.endPos = endPos;
            this.width = width;
        }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - startPos,
                direction = endPos - startPos;
            direction.Normalize();
            Vector2 orthDir = new(-direction.Y, direction.X);
            float distance = Vector2.Distance(startPos, endPos),
                dirProp = Vector2.Dot(relPos, direction) / distance,
                orthDirProp = Math.Abs(Vector2.Dot(relPos, orthDir) / (width.Value * .5f));
            if (dirProp is < 0 or >= 1 || orthDirProp >= 1)
                return false;
            return Contains(dirProp: dirProp, orthDirProp: orthDirProp);
        }

        /// <param name="dirProp">0 to 1</param>
        /// <param name="orthDirProp">0 to 1</param>
        protected abstract bool Contains(float dirProp, float orthDirProp);

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: Texture,
                position: (startPos + endPos) / 2,
                color: color,
                rotation: C.Rotation(vector: endPos - startPos),
                origin: new Vector2(Texture.Width, Texture.Height) * .5f,
                scale: new Vector2(Vector2.Distance(startPos, endPos) / Texture.Width, width.Value / Texture.Height)
            );
    }
}
