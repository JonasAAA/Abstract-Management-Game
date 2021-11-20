using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace Game1.Shapes
{
    [DataContract]
    public class Arrow : VectorShape
    {
        private static readonly Texture2D triangleTexture;

        static Arrow()
            => triangleTexture = C.LoadTexture(name: "triangle");

        protected override Texture2D Texture
            => triangleTexture;

        public Arrow(Vector2 startPos, Vector2 endPos, float baseWidth)
            : base(startPos: startPos, endPos: endPos, width: baseWidth)
        { }

        protected override bool Contains(float dirProp, float orthDirProp)
            => dirProp + orthDirProp < 1;
    }
}
