using ClipperLib;
using LibTessDotNet;
using static Game1.WorldManager;

namespace Game1.OldGeometry
{
    //[Serializable]
    //public class Planet
    //{
    //    private bool buildingDrawingModeOn, prevMouseDown;
    //    private readonly KeyButton toggleBuildDrawingMode;
    //    private List<IntPoint> vertices;
    //    private VertexBuffer vertexBuffer;
    //    private IndexBuffer indexBuffer;
    //    private readonly BasicEffect basicEffect;
    //    private Tess tessellator;

    //    public Planet()
    //    {
    //        ClipperTrial.TryCommonPerimeter();
    //        buildingDrawingModeOn = false;
    //        prevMouseDown = false;
    //        basicEffect = new(C.graphicsDevice)
    //        {
    //            TextureEnabled = false,
    //            VertexColorEnabled = true,
    //            LightingEnabled = false,
    //            Projection = Matrix.CreateScale(2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, -2f / GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, 1)
    //                * Matrix.CreateTranslation(-1, 1, 0)
    //        };

    //        vertexBuffer = new(C.graphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
    //        vertexBuffer.SetData(new VertexPositionColor[]
    //        {
    //            new(new(-150.5351f, 300.351f, 0), Color.Red),
    //            new(new(100.51651f, 200.561351f, 0), Color.Red),
    //            new(new(-100.56115f, -200.51551f, 0), Color.Red),
    //            new(new(100.51651f, 200.561351f, 0), Color.Yellow),
    //            new(new(-150.5351f, 300.351f, 0), Color.Yellow),
    //            new(new(1000.561f, 200.5115f, 0), Color.Yellow),
    //        });
    //        indexBuffer = new(C.graphicsDevice, typeof(int), 6, BufferUsage.WriteOnly);
    //        indexBuffer.SetData(new int[] { 0, 1, 2, 3, 4, 5 });

    //        tessellator = new();

    //        toggleBuildDrawingMode = new
    //        (
    //            key: Keys.B,
    //            action: () =>
    //            {
    //                buildingDrawingModeOn = !buildingDrawingModeOn;
    //                if (buildingDrawingModeOn)
    //                {
    //                    vertices = new();
    //                    tessellator = new();
    //                    return;
    //                }
    //                if (vertices.Count < 3)
    //                {
    //                    vertices = null;
    //                    return;
    //                }
                    
    //                tessellator.AddContour((from vertex in vertices
    //                                        select new ContourVertex(new Vec3(vertex.X, vertex.Y, 0), Color.Red)).ToArray());
    //                tessellator.Tessellate();

    //                vertexBuffer = new(C.graphicsDevice, typeof(VertexPositionColor), tessellator.Vertices.Length, BufferUsage.WriteOnly);
    //                vertexBuffer.SetData
    //                (
    //                    data: (from vertex in tessellator.Vertices
    //                           select new VertexPositionColor(new Vector3(vertex.Position.X, vertex.Position.Y, 0), new Color((double)C.Random(0, 1), (double)C.Random(0, 1), (double)C.Random(0, 1), 1) /*Color.Red*/)).ToArray()
    //                );
    //                indexBuffer = new(C.graphicsDevice, typeof(int), tessellator.ElementCount * 3, BufferUsage.WriteOnly);
    //                indexBuffer.SetData(tessellator.Elements);
    //            }
    //        );
    //    }

    //    public void Update()
    //    {
    //        toggleBuildDrawingMode.Update();

    //        if (!buildingDrawingModeOn)
    //            return;

    //        bool mouseDown = Mouse.GetState().RightButton == ButtonState.Pressed;
    //        if (prevMouseDown && !mouseDown)
    //            vertices.Add
    //            (
    //                item: new IntPoint
    //                (
    //                    x: CurWorldManager.MouseWorldPos.X,
    //                    y: CurWorldManager.MouseWorldPos.Y
    //                )
    //            );

    //        prevMouseDown = mouseDown;
    //    }

    //    public void DrawPoly(Matrix worldToScreenTransform)
    //    {
    //        // to correctly draw clockwise and counterclocwise triangles
    //        C.graphicsDevice.RasterizerState = new()
    //        {
    //            MultiSampleAntiAlias = true,
    //            CullMode = CullMode.None
    //        };
    //        // temporary
    //        //if (vertices is null || buildingDrawingModeOn)
    //        //    return;

    //        C.graphicsDevice.SetVertexBuffer(vertexBuffer);
    //        C.graphicsDevice.Indices = indexBuffer;

    //        basicEffect.View = worldToScreenTransform;

    //        foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
    //        {
    //            effectPass.Apply();
    //            C.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
    //        }

    //        //foreach (var effectPass in basicEffect.CurrentTechnique.Passes)
    //        //{
    //        //    effectPass.Apply();
    //        //    C.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, tessellator.ElementCount);
    //        //}
    //    }
    //}
}
