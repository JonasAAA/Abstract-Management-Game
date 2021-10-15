﻿using Game1.UI;
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

        public readonly ReadOnlyDictionary<Vector2, Node> posToNode;
        public readonly TimeSpan maxLinkTravelTime;
        public readonly double maxLinkJoulesPerKg;

        //private readonly ReadOnlyDictionary<(Vector2, Vector2), double> personDists;
        //private readonly ReadOnlyDictionary<(Vector2, Vector2), double> resDists;

        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        private readonly ReadOnlyDictionary<(Vector2, Vector2), Link> personFirstLinks;
        private readonly ReadOnlyDictionary<(Vector2, Vector2), Link> resFirstLinks;

        private readonly List<Star> stars;
        private readonly List<Node> nodes;
        private readonly List<Link> links;

        public Graph(IEnumerable<Star> stars, IEnumerable<Node> nodes, IEnumerable<Link> links)
            : base(shape: new InfinitePlane())
        {
            this.stars = stars.ToMyHashSet().ToList();
            this.nodes = nodes.ToMyHashSet().ToList();
            this.links = links.ToMyHashSet().ToList();
            foreach (var link in this.links)
            {
                link.node1.AddLink(link: link);
                link.node2.AddLink(link: link);
            }

            maxLinkTravelTime = this.links.Max(link => link.TravelTime);
            maxLinkJoulesPerKg = this.links.Max(link => link.JoulesPerKg);

            (_, personFirstLinks) = FindShortestPaths(distTimeCoeff: CurWorldConfig.personDistanceTimeCoeff, distEnergyCoeff: CurWorldConfig.personDistanceEnergyCoeff);
            (_, resFirstLinks) = FindShortestPaths(distTimeCoeff: CurWorldConfig.resDistanceTimeCoeff, distEnergyCoeff: CurWorldConfig.resDistanceEnergyCoeff);
            posToNode = new
            (
                dictionary: nodes.ToDictionary
                (
                    keySelector: nodes => nodes.Position
                )
            );
        }

        protected override void InitUninitialized()
        {
            base.InitUninitialized();

            // I call initialize on stars, nodes and links as they may add new children to this
            foreach (var star in stars)
            {
                AddChild(child: star, layer: CurWorldConfig.lightLayer);
                star.Initialize();
            }
            foreach (var node in nodes)
            {
                AddChild(child: node, layer: CurWorldConfig.nodeLayer);
                node.Initialize();
            }
            foreach (var link in links)
            {
                AddChild(child: link, layer: CurWorldConfig.linkLayer);
                link.Initialize();
            }
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
                    posToNode: posToNode,
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
                        posToNode: posToNode,
                        resInd: resInd,
                        maxExtraResFunc: MaxExtraRes
                    );
                    nodeInfo.isSplitAleady = true;
                }
        }

        public void DrawBeforeLight()
        {
            foreach (var child in Children(maxLayer: CurWorldConfig.lightLayer - 1))
                child.Draw();
        }

        public void DrawAfterLight()
        {
            foreach (var child in Children(minLayer: CurWorldConfig.lightLayer + 1))
                child.Draw();
        }

        public override void Draw()
            => throw new InvalidOperationException();
    }
}
