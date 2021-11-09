using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class Triangle : NearRectangle
    {
        public enum Direction
        {
            Up = -1,
            Left = 2,
            Down = 1,
            Right = 0
        }

        private static readonly Texture2D triangleTexture;
        static Triangle()
            => triangleTexture = C.LoadTexture(name: "triangle");

        private Vector2 BasePos
            => Center - dirVector * MainAltitudeLength * .5f;
        private float BaseLength
            => ((int)direction % 2) switch
            {
                0 => Height,
                not 0 => Width
            };
        private float MainAltitudeLength
            => ((int)direction % 2) switch
            {
                0 => Width,
                not 0 => Height
            };

        [DataMember] private readonly Direction direction;
        [DataMember] private readonly float rotation;
        [DataMember] private readonly Vector2 origin, dirVector, orthDir, scale;

        public Triangle(float width, float height, Direction direction)
            : base(width: width, height: height)
        {
            this.direction = direction;
            rotation = (int)direction * MathHelper.PiOver2;
            origin = new Vector2(triangleTexture.Width, triangleTexture.Height) * .5f;
            dirVector = C.Direction(rotation: rotation);
            orthDir = new Vector2(-dirVector.Y, dirVector.X);
            scale = new
            (
                x: MainAltitudeLength / triangleTexture.Height,
                y: BaseLength / triangleTexture.Width
            );
        }

        public override bool Contains(Vector2 position)
        {
            Vector2 relPos = position - BasePos;
            float dirProp = Vector2.Dot(relPos, dirVector) / MainAltitudeLength,
                orthDirProp = Math.Abs(Vector2.Dot(relPos, orthDir) / (BaseLength * .5f));
            if (dirProp is < 0 or >= 1 || orthDirProp >= 1)
                return false;
            return dirProp + orthDirProp < 1;
        }

        protected override void Draw(Color color)
            => C.Draw
            (
                texture: triangleTexture,
                position: Center,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale
            );
    }
}
