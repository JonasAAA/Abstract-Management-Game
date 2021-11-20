using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class LightPolygon
    {
        [DataMember] private Vector2 center;
        [DataMember] private List<Vector2> vertices;
        [DataMember] private VertexPositionColorTexture[] vertPosTex;
        [DataMember] private int[] ind;

        [DataMember] private readonly float strength;
        [DataMember] private readonly Color color;

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

        public void Draw(GraphicsDevice graphicsDevice, Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
        {
            if (vertices.Count is 0)
                return;
            Vector2 textureCenter = new(.5f);
            int centerInd = vertices.Count;
            vertPosTex[centerInd] = new VertexPositionColorTexture(Transform(pos: center), color, textureCenter);
            for (int i = 0; i < centerInd; i++)
                vertPosTex[i] = new VertexPositionColorTexture(Transform(vertices[i]), color, textureCenter + (vertices[i] - center) / CurWorldConfig.lightTextureWidth / strength);
            if (vertPosTex.Length == 0)
                return;

            foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertPosTex, 0, vertPosTex.Length, ind, 0, ind.Length / 3);
            }

            Vector3 Transform(Vector2 pos)
            {
                Vector2 transPos = Vector2.Transform(pos, worldToScreenTransform);
                return new Vector3(2 * transPos.X / actualScreenWidth - 1, 1 - 2 * transPos.Y / actualScreenHeight, 0);
            }
        }
    }
}
