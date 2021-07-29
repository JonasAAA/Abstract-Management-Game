using System;
using System.Collections.Generic;

namespace Game1
{
    public sealed class PlayState
    {
        private readonly Graph graph;

        public PlayState()
        {
            graph = new Graph();
            List<Node> nodes = new()
            {
                new
                (
                    state: new(position: new(300, 300)),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
                new
                (
                    state: new(position: new(-500, 300)),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
                new
                (
                    state: new(position: new(0, -200)),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
                new
                (
                    state: new(position: new(500, 0)),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
            };

            List<Link> links = new()
            {
                new
                (
                    node1: nodes[0],
                    node2: nodes[1],
                    travelTime: TimeSpan.FromSeconds(15),
                    minSafeDist: 100
                ),
                new
                (
                    node1: nodes[0],
                    node2: nodes[2],
                    travelTime: TimeSpan.FromSeconds(5),
                    minSafeDist: 100
                ),
                new
                (
                    node1: nodes[2],
                    node2: nodes[1],
                    travelTime: TimeSpan.FromSeconds(5),
                    minSafeDist: 100
                ),
                new
                (
                    node1: nodes[0],
                    node2: nodes[3],
                    travelTime: TimeSpan.FromSeconds(5),
                    minSafeDist: 100
                ),
            };

            foreach (var node in nodes)
                graph.AddNode(node);

            foreach (var link in links)
                graph.AddEdge(link);
        }

        public void Update()
        {
            C.Camera.Update();
            graph.Update();
        }

        public void Draw()
        {
            C.Camera.BeginDraw();

            graph.Draw();

            C.Camera.EndDraw();
        }
    }
}
