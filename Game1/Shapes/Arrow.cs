﻿using Game1.PrimitiveTypeWrappers;

namespace Game1.Shapes
{
    [Serializable]
    public class Arrow : VectorShape
    {
        private static readonly Texture2D triangleTexture;

        static Arrow()
            => triangleTexture = C.LoadTexture(name: "triangle");

        protected override Texture2D Texture
            => triangleTexture;

        public Arrow(Vector2 startPos, Vector2 endPos, IReadOnlyChangingUFloat baseWidth)
            : base(startPos: startPos, endPos: endPos, width: baseWidth)
        { }

        protected override bool Contains(float dirProp, float orthDirProp)
            => dirProp + orthDirProp < 1;
    }
}
