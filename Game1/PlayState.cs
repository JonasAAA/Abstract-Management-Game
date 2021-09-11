using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public sealed class PlayState
    {
        public PlayState()
        {
            const int width = 8, height = 5, dist = 200;
            Node[,] nodes = new Node[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    nodes[i, j] = new
                    (
                        state: new
                        (
                            position: new((i - (width - 1) * .5f) * dist, (j - (height - 1) * .5f) * dist),
                            maxBatchDemResStored: 2
                        ),
                        image: new
                        (
                            imageName: "node",
                            width: 64
                        ),
                        startPersonCount: 5
                    );

            const int minSafeDist = 100;
            const double distScale = .1;

            List<Link> links = new();
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height - 1; j++)
                    links.Add
                    (
                        item: new
                        (
                            node1: nodes[i, j],
                            node2: nodes[i, j + 1],
                            travelTime: TimeSpan.FromSeconds((i + 1) * distScale),
                            wattsPerKg: (j + 1.5) * distScale,
                            minSafeDist: minSafeDist
                        )
                    );

            for (int i = 0; i < width - 1; i++)
                for (int j = 0; j < height; j++)
                    links.Add
                    (
                        item: new
                        (
                            node1: nodes[i, j],
                            node2: nodes[i + 1, j],
                            travelTime: TimeSpan.FromSeconds((i + 1.5) * distScale),
                            wattsPerKg: (j + 1) * distScale,
                            minSafeDist: minSafeDist
                        )
                    );
            Graph.Initialize
            (
                nodes: from Node node in nodes
                       select node,
                links: links,
                overlay: Overlay.Res0
            );

            UIPanel horizPanel = new UIHorizPanel();

            horizPanel.AddChild
            (
                child: new UIRectangle
                (
                    width: 200,
                    height: 100,
                    color: Color.Yellow
                )
            );

            horizPanel.AddChild
            (
                child: new UIRectangle
                (
                    width: 150,
                    height: 150,
                    color: Color.Blue
                )
            );

            UIPanel panel = new UIVertPanel();
            panel.AddChild
            (
                child: new UIRectangle
                (
                    width: 100,
                    height: 200,
                    color: Color.Green
                )
            );

            panel.AddChild
            (
                child: new UIRectangle
                (
                    width: 150,
                    height: 150,
                    color: Color.Red
                )
            );

            panel.AddChild(child: horizPanel);

            panel.TopLeftCorner = Vector2.Zero;

            ActiveUI.Add(UIElement: panel);
        }

        public void Update(TimeSpan elapsed)
        {
            C.Camera.Update();

            ActiveUI.Update();

            Graph.Update(elapsed: elapsed);
        }

        public void Draw()
        {
            C.Camera.BeginDraw();
            Graph.Draw();
            C.Camera.EndDraw();

            C.SpriteBatch.Begin();
            Graph.DrawHUD();
            ActiveUI.DrawHUD();
            C.SpriteBatch.End();
        }
    }
}
