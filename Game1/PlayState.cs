﻿using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public sealed class PlayState
    {
        private readonly LightSource lightSource;

        public PlayState()
        {
            lightSource = new(position: Vector2.Zero, strength: 2, color: Color.Yellow);

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
                        disk: new DiskWithShadow(radius: 32),
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
                nodes: from Node node in nodes
                       select node,
                links: links,
                overlay: Overlay.Res0
            );
        }

        public void Update(TimeSpan elapsed)
        {
            C.WorldCamera.Update(elapsed: elapsed, canScroll: !ActiveUI.MouseAboveHUD);

            ActiveUI.Update(elapsed: elapsed);

            lightSource.position = MyMouse.WorldPos;

            Graph.World.Update(elapsed: elapsed);

            LightManager.Update();
        }

        public void Draw()
        {
            LightManager.Draw();
            ActiveUI.Draw();
        }
    }
}
