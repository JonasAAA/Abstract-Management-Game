using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class Arrow : Shape
    {
        private static readonly Texture2D triangleTexture;

        static Arrow()
            => triangleTexture = C.LoadTexture(name: "triangle");

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

        [DataMember] public readonly Vector2 startPos, endPos;
        [DataMember] private readonly float width;

        public Arrow(Vector2 startPos, Vector2 endPos, float width)
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
                orthDirProp = Math.Abs(Vector2.Dot(relPos, orthDir) / (width * .5f));
            if (dirProp is < 0 or >= 1 || orthDirProp >= 1)
                return false;
            return dirProp + orthDirProp < 1;
        }

        protected override void Draw(Color color)
            => DrawArrow(startPos: startPos, endPos: endPos, width: width, color: color);
    }
}
