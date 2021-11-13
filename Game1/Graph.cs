using Game1.Events;
using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class Graph : UIElement, IChoiceChangedListener<Overlay>, IActiveChangedListener
    {
        [DataContract]
        private class NodeInfo
        {
            private static int resInd;

            public static void Init(int resInd)
                => NodeInfo.resInd = resInd;

            [DataMember] public readonly Node node;
            [DataMember] public readonly List<NodeInfo> nodesIn, nodesOut;
            [DataMember] public uint unvisitedDestinsCount;
            [DataMember] public bool isSplitAleady;

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

        public IEnumerable<Node> Nodes
            => nodes;

        [DataMember] public readonly ReadOnlyDictionary<Vector2, Node> posToNode;
        [DataMember] public readonly TimeSpan maxLinkTravelTime;
        [DataMember] public readonly double maxLinkJoulesPerKg;

        public override bool CanBeClicked
            => true;

        private IEnumerable<WorldUIElement> WorldUIElements
        {
            get
            {
                foreach (var star in stars)
                    yield return star;
                foreach (var node in nodes)
                    yield return node;
                foreach (var link in links)
                    yield return link;
                for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
                    foreach (var resDestinArrow in resDestinArrows[resInd])
                        yield return resDestinArrow;
            }
        }

        [DataMember] private readonly ReadOnlyDictionary<(Vector2, Vector2), double> personDists;
        //[DataMember] private readonly ReadOnlyDictionary<(Vector2, Vector2), double> resDists;

        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        [DataMember] private readonly ReadOnlyDictionary<(Vector2, Vector2), Link> personFirstLinks;
        [DataMember] private readonly ReadOnlyDictionary<(Vector2, Vector2), Link> resFirstLinks;

        [DataMember] private readonly List<Star> stars;
        [DataMember] private readonly List<Node> nodes;
        [DataMember] private readonly List<Link> links;

        [DataMember] private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDestinArrows;
        [DataMember] public WorldUIElement ActiveWorldElement { get; private set; }

        public Graph(IEnumerable<Star> stars, IEnumerable<Node> nodes, IEnumerable<Link> links)
            : base(shape: new InfinitePlane(color: Color.Black))
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

            foreach (var star in stars)
                AddChild(child: star, layer: CurWorldConfig.lightLayer);
            foreach (var node in nodes)
                AddChild(child: node, layer: CurWorldConfig.nodeLayer);
            foreach (var link in links)
                AddChild(child: link, layer: CurWorldConfig.linkLayer);

            resDestinArrows = new();
            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
                resDestinArrows[resInd] = new();

            if (CurWorldManager.Overlay <= MaxRes)
                AddChild
                (
                    child: resDestinArrows[(int)CurWorldManager.Overlay],
                    layer: CurWorldConfig.resDistribArrowsUILayer
                );

            foreach (var worldUIElement in WorldUIElements)
                worldUIElement.activeChanged.Add(listener: this);

            ActiveWorldElement = null;

            CurOverlayChanged.Add(listener: this);
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

        public void AddResDestinArrow(int resInd, ResDestinArrow resDestinArrow)
        {
            resDestinArrow.activeChanged.Add(listener: this);
            resDestinArrows[resInd].AddChild(child: resDestinArrow);
        }

        public override void OnClick()
        {
            base.OnClick();
    
            ActiveWorldElement.Active = false;
        }

        public void RemoveResDestinArrow(int resInd, ResDestinArrow resDestinArrow)
        {
            resDestinArrow.activeChanged.Remove(listener: this);
            resDestinArrows[resInd].RemoveChild(child: resDestinArrow);
        }

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

        void IChoiceChangedListener<Overlay>.ChoiceChangedResponse(Overlay prevOverlay)
        {
            if (prevOverlay <= MaxRes)
                RemoveChild(child: resDestinArrows[(int)prevOverlay]);
            if (CurWorldManager.Overlay <= MaxRes)
                AddChild
                (
                    child: resDestinArrows[(int)CurWorldManager.Overlay],
                    layer:CurWorldConfig.resDistribArrowsUILayer
                );
        }

        void IActiveChangedListener.ActiveChangedResponse(WorldUIElement worldUIElement)
        {
            if (CurWorldManager.ArrowDrawingModeOn)
            {
                if (worldUIElement.Active)
                {
                    ((Node)ActiveWorldElement).AddResDestin(destination: ((Node)worldUIElement).Position);
                    worldUIElement.Active = false;
                }
                return;
            }

            if (worldUIElement.Active)
            {
                if (ActiveWorldElement is not null)
                    ActiveWorldElement.Active = false;
                ActiveWorldElement = worldUIElement;
            }
            else
            {
                if (ActiveWorldElement == worldUIElement)
                    ActiveWorldElement = null;
            }
        }
    }
}
