using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class Graph : UIElement<IUIElement>, IChoiceChangedListener<IOverlay>, IActiveChangedListener
    {
        [Serializable]
        private class NodeInfo
        {
            private static ResInd resInd;

            public static void Init(ResInd resInd)
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

        public IEnumerable<Node> Nodes
            => nodes;

        public readonly ReadOnlyDictionary<MyVector2, Node> posToNode;
        public readonly TimeSpan maxLinkTravelTime;
        public readonly UDouble maxLinkJoulesPerKg;

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
                foreach (var resDestinArrowsByRes in resDestinArrows)
                    foreach (var resDestinArrow in resDestinArrowsByRes)
                        yield return resDestinArrow;
            }
        }

        //private readonly ReadOnlyDictionary<(MyVector2, MyVector2), double> personDists;
        //private readonly ReadOnlyDictionary<(MyVector2, MyVector2), double> resDists;

        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        private readonly ReadOnlyDictionary<(MyVector2, MyVector2), Link> personFirstLinks;
        private readonly ReadOnlyDictionary<(MyVector2, MyVector2), Link> resFirstLinks;

        private readonly List<Star> stars;
        private readonly List<Node> nodes;
        private readonly List<Link> links;

        private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDestinArrows;

        public WorldUIElement ActiveWorldElement { get; private set; }

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
            foreach (var resInd in ResInd.All)
                resDestinArrows[resInd] = new();

            if (CurWorldManager.Overlay is ResInd singleResInd)
                AddChild
                (
                    child: resDestinArrows[singleResInd],
                    layer: CurWorldConfig.resDistribArrowsUILayer
                );

            foreach (var worldUIElement in WorldUIElements)
                worldUIElement.activeChanged.Add(listener: this);

            ActiveWorldElement = null;

            CurOverlayChanged.Add(listener: this);
        }

        // currently uses Floyd-Warshall;
        // Dijkstra would be more efficient
        private (ReadOnlyDictionary<(MyVector2, MyVector2), UDouble> dists, ReadOnlyDictionary<(MyVector2, MyVector2), Link> firstLinks) FindShortestPaths(UDouble distTimeCoeff, UDouble distEnergyCoeff)
        {
            UDouble[,] distsArray = new UDouble[nodes.Count, nodes.Count];
            Link[,] firstLinksArray = new Link[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsArray[i, j] = UDouble.positiveInfinity;
                    firstLinksArray[i, j] = null;
                }

            for (int i = 0; i < nodes.Count; i++)
                distsArray[i, i] = 0;

            foreach (var link in links)
            {
                int i = nodes.IndexOf(link.node1), j = nodes.IndexOf(link.node2);
                Debug.Assert(i >= 0 && j >= 0);
                distsArray[i, j] = distTimeCoeff * (UDouble)link.TravelTime.TotalSeconds + distEnergyCoeff * link.JoulesPerKg;
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

            Dictionary<(MyVector2, MyVector2), UDouble> distsDict = new();
            Dictionary<(MyVector2, MyVector2), Link> firstLinksDict = new();
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

        public void AddResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
        {
            resDestinArrow.activeChanged.Add(listener: this);
            resDestinArrows[resInd].AddChild(child: resDestinArrow);
        }

        public override void OnClick()
        {
            base.OnClick();

            if (ActiveWorldElement is not null)
            {
                ActiveWorldElement.Active = false;
                ActiveWorldElement = null;
            }
        }

        public void RemoveResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
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

            foreach (var resInd in ResInd.All)
                SplitRes(resInd: resInd);

            foreach (var node in nodes)
                node.EndSplitRes(resFirstLinks: resFirstLinks);
        }

        /// <summary>
        /// TODO:
        /// choose random leafs
        /// </summary>
        public void SplitRes(ResInd resInd)
        {
            NodeInfo.Init(resInd: resInd);
            Dictionary<MyVector2, NodeInfo> nodeInfos = nodes.ToDictionary
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

            ulong MaxExtraRes(MyVector2 position)
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

        void IChoiceChangedListener<IOverlay>.ChoiceChangedResponse(IOverlay prevOverlay)
        {
            if (prevOverlay is ResInd prevResInd)
                RemoveChild(child: resDestinArrows[prevResInd]);
            
            if (CurWorldManager.Overlay is ResInd resInd)
                AddChild
                (
                    child: resDestinArrows[resInd],
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
