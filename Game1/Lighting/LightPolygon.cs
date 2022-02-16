﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public class LightPolygon
    {
        private Vector2 center;
        private List<Vector2> vertices;
        private VertexPositionColorTexture[] vertPosTexs;
        private ushort[] inds;

        private readonly float strength;
        private readonly Color color;

        /// <param name="strength">a positive float which determins the radius of the lit area</param>
        public LightPolygon(float strength, Color color)
            : this(strength: strength, color: color, center: Vector2.Zero, vertices: new())
        {
            vertices = new List<Vector2>();
            vertPosTexs = Array.Empty<VertexPositionColorTexture>();
            inds = Array.Empty<ushort>();
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
            vertPosTexs = Array.Empty<VertexPositionColorTexture>();
            inds = Array.Empty<ushort>();
            Update(center: center, vertices: vertices);
        }

        public void Update(Vector2 center, List<Vector2> vertices)
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

        public void Draw(GraphicsDevice graphicsDevice, Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
        {
            if (vertices.Count is 0)
                return;
            Vector2 textureCenter = new(.5f);
            int centerInd = vertices.Count;
            vertPosTexs[centerInd] = new VertexPositionColorTexture(Transform(pos: center), color, textureCenter);
            for (int i = 0; i < centerInd; i++)
                vertPosTexs[i] = new VertexPositionColorTexture(Transform(vertices[i]), color, textureCenter + (vertices[i] - center) / CurWorldConfig.lightTextureWidth / strength);
            if (vertPosTexs.Length == 0)
                return;

            VertexBuffer vertexBuffer = new(graphicsDevice, typeof(VertexPositionColorTexture), vertPosTexs.Length, BufferUsage.WriteOnly);
            IndexBuffer indexBuffer = new(graphicsDevice, typeof(ushort), inds.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertPosTexs);
            indexBuffer.SetData(inds);

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives
                (
                    primitiveType: PrimitiveType.TriangleList,
                    baseVertex: 0,
                    startIndex: 0,
                    primitiveCount: inds.Length / 3
                );
                //graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertPosTexs, 0, vertPosTexs.Length, inds, 0, inds.Length / 3);
            }

            Vector3 Transform(Vector2 pos)
            {
                Vector2 transPos = Vector2.Transform(pos, worldToScreenTransform);
                return new Vector3(2 * transPos.X / actualScreenWidth - 1, 1 - 2 * transPos.Y / actualScreenHeight, 0);
            }
        }
    }
}