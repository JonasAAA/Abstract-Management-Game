using ClipperLib;
using LibTessDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game1.WorldManager;

namespace Game1
{
    public class Planet
    {
        private bool buildingDrawingModeOn, prevMouseDown;
        private readonly KeyButton toggleBuildDrawingMode;
        private List<IntPoint> vertices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private readonly BasicEffect basicEffect;
        private Tess tessellator;

        public Planet()
        {
            buildingDrawingModeOn = false;
            prevMouseDown = false;
            basicEffect = new(C.graphicsDevice)
            {
                TextureEnabled = false,
                VertexColorEnabled = true,
                LightingEnabled = false,
                Projection = Matrix.CreateScale(2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, -2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 1)
                    * Matrix.CreateTranslation(-1, 1, 0)
            };

            tessellator = new();

            toggleBuildDrawingMode = new
            (
                key: Keys.B,
                action: () =>
                {
                    buildingDrawingModeOn = !buildingDrawingModeOn;
                    if (buildingDrawingModeOn)
                    {
                        vertices = new();
                        tessellator = new();
                        return;
                    }
                    if (vertices.Count < 3)
                    {
                        vertices = null;
                        return;
                    }
                    
                    tessellator.AddContour((from vertex in vertices
                                            select new ContourVertex(new Vec3(vertex.X, vertex.Y, 0), Color.Red)).ToArray());
                    tessellator.Tessellate();

                    vertexBuffer = new(C.graphicsDevice, type: typeof(VertexPositionColor), tessellator.Vertices.Length, BufferUsage.WriteOnly);
                    vertexBuffer.SetData
                    (
                        data: (from vertex in tessellator.Vertices
                               select new VertexPositionColor(new Vector3(vertex.Position.X, vertex.Position.Y, 0), new Color((float)C.Random(0, 1), (float)C.Random(0, 1), (float)C.Random(0, 1), 1) /*Color.Red*/)).ToArray()
                    );
                    indexBuffer = new(C.graphicsDevice, typeof(ushort), tessellator.ElementCount * 3, BufferUsage.WriteOnly);
                    indexBuffer.SetData((from ind in tessellator.Elements select (ushort)ind).ToArray());
                }
            );
        }

        public void Update()
        {
            toggleBuildDrawingMode.Update();

            if (!buildingDrawingModeOn)
                return;

            bool mouseDown = Mouse.GetState().RightButton == ButtonState.Pressed;
            if (prevMouseDown && !mouseDown)
                vertices.Add
                (
                    item: new IntPoint
                    (
                        x: CurWorldManager.MouseWorldPos.X,
                        y: CurWorldManager.MouseWorldPos.Y
                    )
                );

            prevMouseDown = mouseDown;
        }

        public void DrawPoly(Matrix worldToScreenTransform)
        {
            // to correctly draw clockwise and counterclocwise triangles
            C.graphicsDevice.RasterizerState = new()
            {
                CullMode = CullMode.None
            };
            // temporary
            if (vertices is null || buildingDrawingModeOn)
                return;

            C.graphicsDevice.SetVertexBuffer(vertexBuffer);
            C.graphicsDevice.Indices = indexBuffer;

            basicEffect.View = worldToScreenTransform;
            
            foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                C.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, tessellator.ElementCount);
            }
        }
    }
}
