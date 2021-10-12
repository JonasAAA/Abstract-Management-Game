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
            Star[] stars = new Star[]
            {
                new
                (
                    radius: 20,
                    center: new Vector2(0, -300),
                    prodWatts: 200,
                    color: Color.Lerp(Color.White, Color.Red, .3f)
                ),
                new
                (
                    radius: 10,
                    center: new Vector2(200, 300),
                    prodWatts: 100,
                    color: Color.Lerp(Color.White, Color.Blue, .3f)
                ),
                new
                (
                    radius: 40,
                    center: new Vector2(-200, 100),
                    prodWatts: 400,
                    color: Color.Lerp(Color.White, new Color(0f, 1f, 0f), .3f)
                ),
            };

            const int width = 8, height = 5, dist = 200;
            Node[,] nodes = new Node[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    nodes[i, j] = new
                    (
                        state: new
                        (
                            position: new Vector2(i - (width - 1) * .5f, j - (height - 1) * .5f) * dist,
                            maxBatchDemResStored: 2
                        ),
                        radius: 32,
                        activeColor: Color.White,
                        inactiveColor: Color.Gray,
                        resDestinArrowWidth: 64
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
            Graph.InitializeWorld
            (
                stars: stars,
                nodes: from Node node in nodes
                       select node,
                links: links,
                overlay: Overlay.Res0
            );

            ActiveUI.Initialize();
        }

        public void Update(TimeSpan elapsed)
        {
            C.WorldCamera.Update(elapsed: elapsed, canScroll: !ActiveUI.MouseAboveHUD);

            ActiveUI.Update(elapsed: elapsed);

            LightManager.Update();

            Graph.World.Update(elapsed: elapsed);
        }

        public void Draw()
        {
            LightManager.Draw();
            ActiveUI.Draw();
        }
    }
}
