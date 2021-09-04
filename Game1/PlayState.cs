﻿using System;
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

            //float radius = 300;

            //List<Node> nodes = new()
            //{
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: 0) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        ),
            //        startPersonCount: 100
            //    ),
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: MathHelper.Pi / 3) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        )
            //    ),
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: MathHelper.Pi * 2 / 3) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        )
            //    ),
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: MathHelper.Pi) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        )
            //    ),
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: MathHelper.Pi * 4 / 3) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        )
            //    ),
            //    new
            //    (
            //        state: new
            //        (
            //            position: new(position: C.Direction(rotation: MathHelper.Pi * 5 / 3) * radius),
            //            maxBatchDemResStored: 2
            //        ),
            //        image: new
            //        (
            //            imageName: "node",
            //            width: 64
            //        )
            //    ),
            //};

            //List<Link> links = new()
            //{
            //    new
            //    (
            //        node1: nodes[0],
            //        node2: nodes[1],
            //        travelTime: TimeSpan.FromSeconds(1),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .6
            //    ),
            //    new
            //    (
            //        node1: nodes[1],
            //        node2: nodes[2],
            //        travelTime: TimeSpan.FromSeconds(2),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .5
            //    ),
            //    new
            //    (
            //        node1: nodes[2],
            //        node2: nodes[3],
            //        travelTime: TimeSpan.FromSeconds(3),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .4
            //    ),
            //    new
            //    (
            //        node1: nodes[3],
            //        node2: nodes[4],
            //        travelTime: TimeSpan.FromSeconds(4),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .3
            //    ),
            //    new
            //    (
            //        node1: nodes[4],
            //        node2: nodes[5],
            //        travelTime: TimeSpan.FromSeconds(5),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .2
            //    ),
            //    new
            //    (
            //        node1: nodes[5],
            //        node2: nodes[0],
            //        travelTime: TimeSpan.FromSeconds(6),
            //        minSafeDist: 100,
            //        reqWattsPerKgPerSec: .1
            //    ),
            //};

            //Graph.Initialize(nodes: nodes, links: links, overlay: Overlay.Res0);
        }

        public void Update(TimeSpan elapsed)
        {
            C.Camera.Update();

            Graph.Update(elapsed: elapsed);
        }

        public void Draw()
        {
            C.Camera.BeginDraw();
            Graph.Draw();
            C.Camera.EndDraw();

            C.SpriteBatch.Begin();
            Graph.DrawHUD();
            C.SpriteBatch.End();
        }
    }
}
