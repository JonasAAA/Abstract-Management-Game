using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public static class Graph
    {
        public static IEnumerable<Node> Nodes
            => nodes;
        public static IEnumerable<Link> Links
            => links;
        public static ReadOnlyDictionary<(Node, Node), double> ElectrDists { get; private set; }
        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        public static ReadOnlyDictionary<(Node, Node), Link> ElectrFirstLinks { get; private set; }

        private static readonly List<Node> nodes;
        private static readonly List<Link> links;
        private static readonly HashSet<Node> nodeSet;
        private static readonly HashSet<Link> linkSet;
        private static readonly ulong ambientWattsPerSec = 100;
        private static IUIElement activeElement;
        private static double reqWattsPerSec, prodWattsPerSec;

        static Graph()
        {
            nodes = new();
            links = new();
            nodeSet = new();
            linkSet = new();
            activeElement = null;
            reqWattsPerSec = 0;
            prodWattsPerSec = ambientWattsPerSec;
        }

        public static void Initialize(List<Node> nodes, List<Link> links)
        {
            foreach (var node in nodes)
                AddNode(node);
            foreach (var link in links)
                AddLink(link);

            FindShortestPaths();
        }

        private static void AddNode(Node node)
        {
            if (nodeSet.Contains(node))
                throw new ArgumentException();
            nodeSet.Add(node);
            nodes.Add(node);
        }

        private static void AddLink(Link link)
        {
            if (linkSet.Contains(link))
                throw new ArgumentException();
            linkSet.Add(link);
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
        }

        // currently uses Floyd-Warshall;
        // Dijkstra would be more efficient
        private static void FindShortestPaths()
        {
            double[,] distsArray = new double[nodes.Count, nodes.Count];
            Link[,] firstLinksArray = new Link[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsArray[i, j] = double.PositiveInfinity;
                    firstLinksArray[i, j] = null;
                }

            for (int i = 0; i < nodes.Count; i++)
                distsArray[i, i] = 0;

            foreach (var link in links)
            {
                int i = nodes.IndexOf(link.node1), j = nodes.IndexOf(link.node2);
                Debug.Assert(i >= 0 && j >= 0);
                distsArray[i, j] = link.WattsPerKg;
                distsArray[j, i] = distsArray[i, j];
                firstLinksArray[i, j] = link;
                firstLinksArray[j, i] = link;
            }

            for (int k = 0; k < nodes.Count; k++)
                for (int i = 0; i < nodes.Count; i++)
                    for (int j = 0; j < nodes.Count; j++)
                        if (i != k && distsArray[i, j] > distsArray[i, k] + distsArray[k, j])
                        {
                            distsArray[i, j] = distsArray[i, k] + distsArray[k, j];
                            firstLinksArray[i, j] = firstLinksArray[i, k];
                            Debug.Assert(firstLinksArray[i, j] is not null);
                        }

            Dictionary<(Node, Node), double> distsDict = new();
            Dictionary<(Node, Node), Link> firstLinksDict = new();
            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsDict.Add
                    (
                        key: (nodes[i], nodes[j]),
                        value: distsArray[i, j]
                    );
                    firstLinksDict.Add
                    (
                        key: (nodes[i], nodes[j]),
                        value: firstLinksArray[i, j]
                    );
                }

            ElectrDists = new(distsDict);
            ElectrFirstLinks = new(firstLinksDict);
        }

        public static void Update(GameTime gameTime)
        {
            reqWattsPerSec = nodes.Sum(node => node.ReqWattsPerSec()) + links.Sum(link => link.ReqWattsPerSec());
            prodWattsPerSec = ambientWattsPerSec + nodes.Sum(node => node.ProdWattsPerSec());

            //double electrPropor = Math.Min(1, prodWattsPerSec / reqWattsPerSec);
            //Debug.Assert(electrPropor is >= 0 and <= 1);

            //throw new NotImplementedException();
            if (reqWattsPerSec > prodWattsPerSec)
                C.Update(elapsed: gameTime.ElapsedGameTime * prodWattsPerSec / reqWattsPerSec);
            else
                C.Update(elapsed: gameTime.ElapsedGameTime);

            links.ForEach(link => link.StartUpdate());

            nodes.ForEach(node => node.StartUpdate());

            links.ForEach(link => link.EndUpdate());

            nodes.ForEach(node => node.EndUpdate());

            if (MyMouse.RightClick)
            {
                activeElement = null;
                return;
            }
            if (MyMouse.LeftClick)
            {
                activeElement = null;
                foreach (var element in nodes.Cast<IUIElement>().Concat(links))
                    if (element.Contains(position: MyMouse.Position))
                    {
                        activeElement = element;
                        break;
                    }
            }

            if (activeElement is not null)
                activeElement.ActiveUpdate();

            JobMatching.Match();
        }

        public static void Draw()
        {
            foreach (var link in links)
                link.Draw(active: ReferenceEquals(link, activeElement));

            foreach (var node in nodes)
                node.Draw(active: node == activeElement);
        }

        public static void DrawHUD()
        {
            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: $"required: {reqWattsPerSec:0.##}\nproduced: {prodWattsPerSec:0.##}",
                position: new Vector2(10, 10),
                color: Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: .15f,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }
}
