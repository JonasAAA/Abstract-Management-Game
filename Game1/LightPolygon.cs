using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class LightPolygon
    {
        private Vector2 center;
        private List<Vector2> vertices;
        private VertexPositionColorTexture[] vertPosTex;
        private int[] ind;

        private readonly float strength;
        private readonly Color color;

        /// <param name="strength">a positive float which determins the radius of the lit area</param>
        public LightPolygon(float strength, Color color)
            : this(strength: strength, color: color, center: Vector2.Zero, vertices: new())
        {
            vertices = new List<Vector2>();
            vertPosTex = Array.Empty<VertexPositionColorTexture>();
            ind = Array.Empty<int>();
            if (strength <= 0)
                throw new ArgumentOutOfRangeException();
            this.strength = strength;
            this.color = color;
        }

        public LightPolygon(float strength, Color color, Vector2 center, List<Vector2> vertices)
        {
            if (strength <= 0)
                throw new ArgumentOutOfRangeException();
            this.strength = strength;
            this.color = color;
            this.center = center;
            this.vertices = vertices;
            vertPosTex = Array.Empty<VertexPositionColorTexture>();
            ind = Array.Empty<int>();
            Update(center: center, vertices: vertices);
        }

        public void Update(Vector2 center, List<Vector2> vertices)
        {
            this.center = center;
            this.vertices = vertices;
            int centerInd = vertices.Count;
            vertPosTex = new VertexPositionColorTexture[centerInd + 1];

            ind = new int[vertices.Count * 3];
            for (int i = 0; i < vertices.Count; i++)
            {
                // may need to swap the last two
                ind[3 * i] = centerInd;
                ind[3 * i + 1] = i;
                ind[3 * i + 2] = (i + 1) % vertices.Count;
            }
        }

        public void Draw()
        {
            Vector2 textureCenter = new(.5f);
            int centerInd = vertices.Count;
            vertPosTex[centerInd] = new VertexPositionColorTexture(LightManager.Transform(pos: center), color, textureCenter);
            for (int i = 0; i < centerInd; i++)
                vertPosTex[i] = new VertexPositionColorTexture(LightManager.Transform(vertices[i]), color, textureCenter + (vertices[i] - center) / LightManager.maxWidth / strength);
            if (vertPosTex.Length == 0)
                return;

            //C.GraphicsDevice.RasterizerState = new RasterizerState()
            //{
            //    CullMode = CullMode.None
            //};

            //C.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            //GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var effectPass in LightManager.BasicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                C.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertPosTex, 0, vertPosTex.Length, ind, 0, ind.Length / 3);
            }
        }
    }
}
