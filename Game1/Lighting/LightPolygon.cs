namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightPolygon
    {
        private static readonly Effect starLightEffect = C.ContentManager.Load<Effect>("StarLight");

        [Serializable]
        public readonly record struct LightSourceInfo(MyVector2 Center, Length Radius, UDouble LightAmount);

        private LightSourceInfo lightSourceInfo;
        private List<MyVector2> vertices;
        private VertexPositionColor[] vertPosTexs;
        private ushort[] inds;

        public LightPolygon()
        {
            vertices = [];
            vertPosTexs = [];
            inds = [];
        }

        public void Update(LightSourceInfo lightSourceInfo, List<MyVector2> vertices)
        {
            this.lightSourceInfo = lightSourceInfo;
            this.vertices = vertices;
            ushort centerInd = (ushort)vertices.Count;
            vertPosTexs = new VertexPositionColor[centerInd + 1];

            inds = new ushort[vertices.Count * 3];
            for (ushort i = 0; i < vertices.Count; i++)
            {
                // may need to swap the last two
                inds[3 * i] = centerInd;
                inds[3 * i + 1] = i;
                inds[3 * i + 2] = (ushort)((i + 1) % vertices.Count);
            }
        }

        public void Draw(Matrix worldToScreenTransform, Color color, int actualScreenWidth, int actualScreenHeight)
        {
            if (vertices.Count is 0)
                return;
            int centerInd = vertices.Count;
            vertPosTexs[centerInd] = new(Transform(lightSourceInfo.Center), color);
            for (int i = 0; i < centerInd; i++)
                vertPosTexs[i] = new(Transform(vertices[i]), color);
            if (vertPosTexs.Length == 0)
                return;

            // TODO(performance): these are allocated every frame. Look into reusing them to not generate as much garbage for GC
            // In fact, could possibly draw all light polygons in one go (similar what SpriteBatch does with sprites)
            using VertexBuffer vertexBuffer = new(C.GraphicsDevice, typeof(VertexPositionColor), vertPosTexs.Length, BufferUsage.WriteOnly);
            IndexBuffer indexBuffer = new(C.GraphicsDevice, typeof(ushort), inds.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertPosTexs);
            indexBuffer.SetData(inds);

            C.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            C.GraphicsDevice.Indices = indexBuffer;

            starLightEffect.CurrentTechnique = starLightEffect.Techniques["BasicColorDrawing"];
            starLightEffect.Parameters["WorldViewProjection"].SetValue
            (
                worldToScreenTransform
                    * Matrix.CreateScale(new Vector3(2f / actualScreenWidth, -2f / actualScreenHeight, 1))
                    * Matrix.CreateTranslation(new Vector3(-1f, 1f, 0))
            );
            starLightEffect.Parameters["Center"].SetValue(Transform(lightSourceInfo.Center));
            starLightEffect.Parameters["Radius"].SetValue((float)lightSourceInfo.Radius.valueInM);
            starLightEffect.Parameters["LightAmount"].SetValue((float)lightSourceInfo.LightAmount);

            foreach (var effectPass in starLightEffect.CurrentTechnique.Passes)
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

            static Vector3 Transform(MyVector2 pos)
            {
                var posFloat = (Vector2)pos;
                return new Vector3(posFloat.X, posFloat.Y, 0);
            }
        }
    }
}
