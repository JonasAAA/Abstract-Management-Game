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
                    position: new(300, 300),
                    state: new(),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
                new
                (
                    position: new(-500, 300),
                    state: new(),
                    image: new
                    (
                        imageName: "node",
                        width: 64
                    )
                ),
                new
                (
                    position: new(0, -200),
                    state: new(),
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
                    travelTime: TimeSpan.FromSeconds(5)
                ),
                new
                (
                    node1: nodes[0],
                    node2: nodes[2],
                    travelTime: TimeSpan.FromSeconds(5)
                ),
                new
                (
                    node1: nodes[2],
                    node2: nodes[1],
                    travelTime: TimeSpan.FromSeconds(5)
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
