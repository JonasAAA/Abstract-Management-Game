using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static Game1.WorldManager;

namespace Game1
{
    public class Graph : UIElement
    {
        public IEnumerable<Node> Nodes
            => nodes;
        public IEnumerable<Link> Links
            => links;
        private ReadOnlyDictionary<(Vector2, Vector2), double> personDists;
        private ReadOnlyDictionary<(Vector2, Vector2), double> resDists;
        public TimeSpan MaxLinkTravelTime { get; private set; }
        public double MaxLinkJoulesPerKg { get; private set; }
        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        private ReadOnlyDictionary<(Vector2, Vector2), Link> personFirstLinks;
        private ReadOnlyDictionary<(Vector2, Vector2), Link> resFirstLinks;
        public ReadOnlyDictionary<Vector2, Node> PosToNode { get; private set; }

        private readonly List<Star> stars;
        private readonly List<Node> nodes;
        private readonly List<Link> links;
        private readonly double persDistTimeCoeff, persDistEnergyCoeff, resDistTimeCoeff, resDistEnergyCoeff;

        public Graph()
            : base(shape: new InfinitePlane())
        {
            stars = new();
            nodes = new();
            links = new();
            persDistTimeCoeff = 1;
            persDistEnergyCoeff = 0;
            resDistTimeCoeff = 0;
            resDistEnergyCoeff = 1;
        }

        public void Initialize(IEnumerable<Star> stars, IEnumerable<Node> nodes, IEnumerable<Link> links)
        {
            foreach (var star in stars)
                AddStar(star: star);
            foreach (var node in nodes)
                AddNode(node: node);
            foreach (var link in links)
                AddLink(link: link);

            MaxLinkTravelTime = this.links.Max(link => link.TravelTime);
            MaxLinkJoulesPerKg = this.links.Max(link => link.JoulesPerKg);

            (personDists, personFirstLinks) = FindShortestPaths(distTimeCoeff: persDistTimeCoeff, distEnergyCoeff: persDistEnergyCoeff);
            (resDists, resFirstLinks) = FindShortestPaths(distTimeCoeff: resDistTimeCoeff, distEnergyCoeff: resDistEnergyCoeff);
            PosToNode = new
            (
                dictionary: nodes.ToDictionary
                (
                    keySelector: nodes => nodes.Position
                )
            );

            foreach (var node in this.nodes)
                node.Init(startPersonCount: 5);
        }

        private void AddStar(Star star)
        {
            if (stars.Contains(star))
                throw new ArgumentException();
            stars.Add(star);
            AddChild(child: star, layer: LightManager.layer);
        }

        private void AddNode(Node node)
        {
            if (nodes.Contains(node))
                throw new ArgumentException();
            nodes.Add(node);
            AddChild(child: node, layer: 10);
        }

        private void AddLink(Link link)
        {
            if (links.Contains(link))
                throw new ArgumentException();
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
            AddChild(child: link, layer: 0);
        }

        // currently uses Floyd-Warshall;
        // Dijkstra would be more efficient
        private (ReadOnlyDictionary<(Vector2, Vector2), double> dists, ReadOnlyDictionary<(Vector2, Vector2), Link> firstLinks) FindShortestPaths(double distTimeCoeff, double distEnergyCoeff)
        {
            if (distTimeCoeff < 0)
                throw new ArgumentOutOfRangeException();
            if (distEnergyCoeff < 0)
                throw new ArgumentOutOfRangeException();

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
                distsArray[i, j] = distTimeCoeff * link.TravelTime.TotalSeconds + distEnergyCoeff * link.JoulesPerKg;
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

            Dictionary<(Vector2, Vector2), double> distsDict = new();
            Dictionary<(Vector2, Vector2), Link> firstLinksDict = new();
            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsDict.Add
                    (
                        key: (nodes[i].Position, nodes[j].Position),
                        value: distsArray[i, j]
                    );
                    firstLinksDict.Add
                    (
                        key: (nodes[i].Position, nodes[j].Position),
                        value: firstLinksArray[i, j]
                    );
                }

            return (dists: new(distsDict), firstLinks: new(firstLinksDict));
        }

        public void AddUIElement(IUIElement UIElement, ulong layer)
            => AddChild(child: UIElement, layer: layer);

        public void RemoveUIElement(IUIElement UIElement)
            => RemoveChild(child: UIElement);

        public void Update()
        {
            links.ForEach(link => link.Update());
            foreach (var node in nodes)
                node.Update(personFirstLinks: personFirstLinks);

            links.ForEach(link => link.UpdatePeople());
            nodes.ForEach(node => node.UpdatePeople());

            nodes.ForEach(node => node.StartSplitRes());

            for (int resInd = 0; resInd < CurResConfig.ResCount; resInd++)
                SplitRes(resInd: resInd);

            foreach (var node in nodes)
                node.EndSplitRes(resFirstLinks: resFirstLinks);
        }

        private class NodeInfo
        {
            private static int resInd;

            public static void Init(int resInd)
                => NodeInfo.resInd = resInd;

            public readonly Node node;
            public readonly List<NodeInfo> nodesIn, nodesOut;
            public uint unvisitedDestinsCount;
            public bool isSplitAleady;

            public NodeInfo(Node node)
            {
                this.node = node;
                nodesIn = new();
                nodesOut = new();
                unvisitedDestinsCount = 0;
                isSplitAleady = false;
            }

            public ulong MaxExtraRes()
                => unvisitedDestinsCount switch
                {
                    0 => DFS().maxExtraRes,
                    > 0 => ulong.MaxValue
                };

            private (ulong maxExtraRes, ulong subgraphUserTargetStoredRes) DFS()
            {
                if (unvisitedDestinsCount is not 0)
                    throw new InvalidOperationException();

                ulong maxExtraResFromNodesOut = 0,
                    userTargetStoredResFromNodesOut = 0;

                foreach (var nodeInfo in nodesOut)
                {
                    var (curMaxExtraRes, curSubgraphUserTargetStoredRes) = nodeInfo.DFS();
                    maxExtraResFromNodesOut += curMaxExtraRes;
                    userTargetStoredResFromNodesOut += curSubgraphUserTargetStoredRes;
                }

                ulong subgraphUserTargetStoredRes = node.TargetStoredResAmount(resInd: resInd) + userTargetStoredResFromNodesOut,
                    targetStoredRes = node.IfStore(resInd: resInd) switch
                    {
                        true => subgraphUserTargetStoredRes,
                        false => node.TargetStoredResAmount(resInd: resInd)
                    };

                return
                (
                    maxExtraRes: (maxExtraResFromNodesOut + targetStoredRes >= node.TotalQueuedRes(resInd: resInd)) switch
                    {
                        true => maxExtraResFromNodesOut + targetStoredRes - node.TotalQueuedRes(resInd: resInd),
                        false => 0
                    },
                    subgraphUserTargetStoredRes: subgraphUserTargetStoredRes
                );
            }
        }

        /// <summary>
        /// TODO:
        /// choose random leafs
        /// </summary>
        public void SplitRes(int resInd)
        {
            NodeInfo.Init(resInd: resInd);
            Dictionary<Vector2, NodeInfo> nodeInfos = nodes.ToDictionary
            (
                keySelector: node => node.Position,
                elementSelector: node => new NodeInfo(node: node)
            );

            foreach (var nodeInfo in nodeInfos.Values)
                foreach (var resDestin in nodeInfo.node.ResDestins(resInd: resInd))
                {
                    var nodeInfoDestin = nodeInfos[resDestin];

                    nodeInfo.unvisitedDestinsCount++;
                    nodeInfo.nodesOut.Add(nodeInfoDestin);
                    nodeInfoDestin.nodesIn.Add(nodeInfo);
                }
            // sinks could use data stucture like from
            // https://stackoverflow.com/questions/5682218/data-structure-insert-remove-contains-get-random-element-all-at-o1
            // to support taking random element in O(1)
            Queue<NodeInfo> sinks = new
            (
                from nodeInfo in nodeInfos.Values
                where nodeInfo.unvisitedDestinsCount is 0
                select nodeInfo
            );

            ulong MaxExtraRes(Vector2 position)
                => nodeInfos[position].MaxExtraRes();

            while (sinks.Count > 0)
            {
                // want to choose random sink instead of this
                NodeInfo sink = sinks.Dequeue();
                sink.node.SplitRes
                (
                    posToNode: PosToNode,
                    resInd: resInd,
                    maxExtraResFunc: MaxExtraRes
                );

                foreach (var nodeInfo in sink.nodesIn)
                {
                    nodeInfo.unvisitedDestinsCount--;
                    if (nodeInfo.unvisitedDestinsCount is 0)
                        sinks.Enqueue(nodeInfo);
                }
                sink.isSplitAleady = true;
            }

            foreach (var nodeInfo in nodeInfos.Values)
                if (!nodeInfo.isSplitAleady)
                {
                    nodeInfo.node.SplitRes
                    (
                        posToNode: PosToNode,
                        resInd: resInd,
                        maxExtraResFunc: MaxExtraRes
                    );
                    nodeInfo.isSplitAleady = true;
                }
        }

        public void DrawBeforeLight()
        {
            foreach (var child in Children(maxLayer: LightManager.layer - 1))
                child.Draw();
        }

        public void DrawAfterLight()
        {
            foreach (var child in Children(minLayer: LightManager.layer + 1))
                child.Draw();
        }

        public override void Draw()
            => throw new InvalidOperationException();
    }
}
