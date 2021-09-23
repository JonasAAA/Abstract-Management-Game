using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.UI
{
    public class Arrow : Shape
    {
        private static readonly Texture2D triangleTexture;
        static Arrow()
            => triangleTexture = C.Content.Load<Texture2D>("triangle");

        public static void DrawArrow(Vector2 startPos, Vector2 endPos, float width, Color color)
            => C.Draw
            (
                texture: triangleTexture,
                position: (startPos + endPos) / 2,
                color: color,
                rotation: C.Rotation(vector: endPos - startPos),
                origin: new Vector2(triangleTexture.Width, triangleTexture.Height) * .5f,
                scale: new Vector2(Vector2.Distance(startPos, endPos) / triangleTexture.Width, width / triangleTexture.Height)
            );

        private readonly Vector2 startPos, endPos;
        private readonly float width;

        public Arrow(Vector2 startPos, Vector2 endPos, float width)
        {
            if (C.IsTiny(Vector2.Distance(startPos, endPos)))
                throw new ArgumentException();
            this.startPos = startPos;
            this.endPos = endPos;
            this.width = width;
        }

        public override bool Contains(Vector2 position)
            => false;

        protected override void Draw(Color color)
            => DrawArrow(startPos: startPos, endPos: endPos, width: width, color: color);
    }
}
