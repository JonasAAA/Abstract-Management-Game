using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightPolygon
    {
        private MyVector2 center;
        private List<MyVector2> vertices;
        private VertexPositionColorTexture[] vertPosTexs;
        private ushort[] inds;

        private readonly UDouble strength;
        private readonly Color color;

        /// <param name="strength">a positive double which determins the radius of the lit area</param>
        public LightPolygon(UDouble strength, Color color)
            : this(strength: strength, color: color, center: MyVector2.zero, vertices: new())
        {
            vertices = new List<MyVector2>();
            vertPosTexs = Array.Empty<VertexPositionColorTexture>();
            inds = Array.Empty<ushort>();
            if (strength.IsCloseTo(other: 0))
                throw new ArgumentOutOfRangeException();
            this.strength = strength;
            this.color = color;
        }

        public LightPolygon(UDouble strength, Color color, MyVector2 center, List<MyVector2> vertices)
        {
            if (strength.IsCloseTo(other: 0))
                throw new ArgumentOutOfRangeException();
            this.strength = strength;
            this.color = color;
            this.center = center;
            this.vertices = vertices;
            vertPosTexs = Array.Empty<VertexPositionColorTexture>();
            inds = Array.Empty<ushort>();
            Update(center: center, vertices: vertices);
        }

        public void Update(MyVector2 center, List<MyVector2> vertices)
        {
            this.center = center;
            this.vertices = vertices;
            ushort centerInd = (ushort)vertices.Count;
            vertPosTexs = new VertexPositionColorTexture[centerInd + 1];

            inds = new ushort[vertices.Count * 3];
            for (ushort i = 0; i < vertices.Count; i++)
            {
                // may need to swap the last two
                inds[3 * i] = centerInd;
                inds[3 * i + 1] = i;
                inds[3 * i + 2] = (ushort)((i + 1) % vertices.Count);
            }
        }

        public void Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
        {
            if (vertices.Count is 0)
                return;
            MyVector2 textureCenter = new(xAndY: .5);
            int centerInd = vertices.Count;
            vertPosTexs[centerInd] = new(Transform(pos: center), color, (Vector2)textureCenter);
            for (int i = 0; i < centerInd; i++)
                vertPosTexs[i] = new(Transform(vertices[i]), color, (Vector2)(textureCenter + (vertices[i] - center) / CurWorldConfig.lightTextureWidthAndHeight / strength));
            if (vertPosTexs.Length == 0)
                return;

            VertexBuffer vertexBuffer = new(C.GraphicsDevice, typeof(VertexPositionColorTexture), vertPosTexs.Length, BufferUsage.WriteOnly);
            IndexBuffer indexBuffer = new(C.GraphicsDevice, typeof(ushort), inds.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertPosTexs);
            indexBuffer.SetData(inds);

            C.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            C.GraphicsDevice.Indices = indexBuffer;

            foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                C.GraphicsDevice.DrawIndexedPrimitives
                (
                    primitiveType: PrimitiveType.TriangleList,
                    baseVertex: 0,
                    startIndex: 0,
                    primitiveCount: inds.Length / 3
                );
            }

            Vector3 Transform(MyVector2 pos)
            {
                MyVector2 transPos = MyVector2.Transform(pos, worldToScreenTransform);
                return new Vector3((float)(2 * transPos.X / actualScreenWidth - 1), (float)(1 - 2 * transPos.Y / actualScreenHeight), 0);
            }
        }
    }
}
