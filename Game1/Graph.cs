using Game1.Delegates;
using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using Game1.ContentHelpers;
using Game1.Collections;

namespace Game1
{
    [Serializable]
    public sealed class Graph : UIElement<IUIElement>, IActiveChangedListener, IWithRealPeopleStats
        //, IChoiceChangedListener<IOverlay>
    {
        [Serializable]
        private sealed class NodeInfo
        {
            private static bool initialized = false;
            private static ResOrRawMatsMix resOrRawMatsMix;

            public static void Init(ResOrRawMatsMix resOrRawMatsMix)
            {
                NodeInfo.resOrRawMatsMix = resOrRawMatsMix;
                initialized = true;
            }

            public readonly CosmicBody node;
            public readonly List<NodeInfo> nodesIn, nodesOut;
            public uint unvisitedDestinsCount;
            public bool isSplitAleady;

            public NodeInfo(CosmicBody node)
            {
                Debug.Assert(initialized);
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

                ulong subgraphUserTargetStoredRes = node.TargetStoredResAmount(resOrRawMatsMix: resOrRawMatsMix) + userTargetStoredResFromNodesOut,
                    targetStoredRes = node.TargetStoredResAmount(resOrRawMatsMix: resOrRawMatsMix);
                // Use logic similar to below if want a not to store some extra resources for the "downstream" nodes.
                //    targetStoredRes = node.IfStore(resOrRawMatsMix: resOrRawMatsMix) switch
                //    {
                //        true => subgraphUserTargetStoredRes,
                //        false => node.TargetStoredResAmount(resOrRawMatsMix: resOrRawMatsMix)
                //    };

                return
                (
                    maxExtraRes: (maxExtraResFromNodesOut + targetStoredRes >= node.TotalQueuedRes(resOrRawMatsMix: resOrRawMatsMix)) switch
                    {
                        true => maxExtraResFromNodesOut + targetStoredRes - node.TotalQueuedRes(resOrRawMatsMix: resOrRawMatsMix),
                        false => 0
                    },
                    subgraphUserTargetStoredRes: subgraphUserTargetStoredRes
                );
            }
        }

        [Serializable]
        private readonly record struct ShortestPaths(EfficientReadOnlyDictionary<(NodeID, NodeID), UDouble> Dists, EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> FirstLinks);

        [Serializable]
        private readonly record struct PersonAndResShortestPaths(ShortestPaths PersonShortestPaths, ShortestPaths ResShortestPaths);

        public IEnumerable<CosmicBody> Nodes
            => nodes;

        public readonly EfficientReadOnlyDictionary<NodeID, CosmicBody> nodeIDToNode;
        public TimeSpan MaxLinkTravelTime { get; private set; }
        public UDouble MaxLinkJoulesPerKg { get; private set; }

        public override bool CanBeClicked
            => true;
        public RealPeopleStats Stats { get; private set; }

        // THIS COLOR IS NOT USED
        protected override Color Color
            => colorConfig.cosmosBackgroundColor;

        private EfficientReadOnlyDictionary<(NodeID, NodeID), UDouble> personDists;
        private EfficientReadOnlyDictionary<(NodeID, NodeID), UDouble> resDists;
        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        private EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks;
        private EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> resFirstLinks;

        private IEnumerable<WorldUIElement> WorldUIElements
        {
            get
            {
                foreach (var node in nodes)
                    yield return node;
                foreach (var link in links)
                    yield return link;
                foreach (var resDestinArrow in resDestinArrows)
                    yield return resDestinArrow;
            }
        }

        private readonly List<CosmicBody> nodes;
        private readonly List<Link> links;

        [NonSerialized] private Task<PersonAndResShortestPaths> shortestPathsTask;
        private readonly UITransparentPanel<ResDestinArrow> resDestinArrows;

        public WorldUIElement? ActiveWorldElement { get; private set; }

        public static Graph CreateFromInfo(FullValidMapInfo mapInfo, WorldCamera mapInfoCamera)
        {
            throw new NotImplementedException();
            //// DIFFICULT to have magicUnlimitedResAmounts as can always create new materials and thus new products
            //// Maybe should just create infinite amount of raw materials and then convert them to more complicated things
            //// However, even the max amount of raw materials is not clear
            //ResPile magicResPile = ResPile.CreateByMagic(amount: ResAmounts.magicUnlimitedResAmounts);
            //Dictionary<string, CosmicBody> cosmicBodiesByName = mapInfo.CosmicBodies.ToDictionary
            //(
            //    keySelector: cosmicBodyInfo => cosmicBodyInfo.Name,
            //    elementSelector: cosmicBodyInfo => new CosmicBody
            //    (
            //        state: new
            //        (
            //            mapInfoCamera: mapInfoCamera,
            //            cosmicBodyInfo: cosmicBodyInfo,
            //            consistsOf: BasicRes.Random(),
            //            resSource: magicResPile
            //        ),
            //        activeColor: colorConfig.selectedWorldUIElementColor
            //        //startingConditions: cosmicBodyInfo.Name == mapInfo.StartingInfo.HouseCosmicBody ?
            //        //(
            //        //    industryFactory: CurIndustryConfig.basicHouseFactory,
            //        //    personCount: CurWorldConfig.startingPersonNumInHouseCosmicBody,
            //        //    resSource: magicResPile
            //        //) : cosmicBodyInfo.Name == mapInfo.StartingInfo.PowerPlantCosmicBody ?
            //        //(
            //        //    industryFactory: CurIndustryConfig.basicPowerPlantFactory,
            //        //    personCount: CurWorldConfig.startingPersonNumInPowerPlantCosmicBody,
            //        //    resSource: magicResPile
            //        //) : null
            //    )
            //);
            //return new
            //(
            //    nodes: cosmicBodiesByName.Values.ToList(),
            //    links: mapInfo.Links.Select
            //    (
            //        linkInfo => new Link
            //        (
            //            node1: cosmicBodiesByName[linkInfo.From],
            //            node2: cosmicBodiesByName[linkInfo.To],
            //            minSafeDist: CurWorldConfig.minSafeDist
            //        )
            //    ).ToList()
            //);
        }

        public Graph(List<CosmicBody> nodes, List<Link> links)
            : base(shape: new InfinitePlane())
        {
            this.nodes = nodes;
            this.links = links;
            
            foreach (var link in this.links)
            {
                link.node1.AddLink(link: link);
                link.node2.AddLink(link: link);
            }

            CalcAndSetMaxLinkStats();

            SetPersonAndResShortestPaths(personAndResShortestPaths: FindPersonAndResShortestPaths(nodes: this.nodes, links: this.links));
            SetShortestPathsTask();

            nodeIDToNode = nodes.ToEfficientReadOnlyDict
            (
                keySelector: node => node.NodeID
            );

            foreach (var node in nodes)
                AddChild(child: node, layer: CurWorldConfig.nodeLayer);
            foreach (var link in links)
                AddChild(child: link, layer: CurWorldConfig.linkLayer);

            resDestinArrows = new();

            AddChild
            (
                child: resDestinArrows,
                layer: CurWorldConfig.resDistribArrowsUILayer
            );
            //if (CurWorldManager.Overlay is IResource singleRes)
            //    AddChild
            //    (
            //        child: resDestinArrows.GetOrCreate(key: singleRes),
            //        layer: CurWorldConfig.resDistribArrowsUILayer
            //    );

            foreach (var worldUIElement in WorldUIElements)
                worldUIElement.activeChanged.Add(listener: this);

            ActiveWorldElement = null;

            //CurOverlayChanged.Add(listener: this);
        }

        public void Initialize()
        {
            if (shortestPathsTask is null)
                SetShortestPathsTask();
        }

        private void CalcAndSetMaxLinkStats()
        {
            MaxLinkTravelTime = links.MaxOrDefault(link => link.TravelTime);
            MaxLinkJoulesPerKg = links.MaxOrDefault(link => link.JoulesPerKg);
        }

        [MemberNotNull(nameof(personDists), nameof(personFirstLinks), nameof(resDists), nameof(resFirstLinks))]
        private void SetPersonAndResShortestPaths(PersonAndResShortestPaths personAndResShortestPaths)
            => ((personDists, personFirstLinks), (resDists, resFirstLinks)) = personAndResShortestPaths;

        [MemberNotNull(nameof(shortestPathsTask))]
        private void SetShortestPathsTask()
            => shortestPathsTask = Task<PersonAndResShortestPaths>.Factory.StartNew
            (
                function: state =>
                {
                    var (nodes, links) = (ValueTuple<List<CosmicBody>, List<Link>>)state!;
                    return FindPersonAndResShortestPaths(nodes: nodes, links: links);
                },
                // TODO: decide if it's needed. it probably executes the function on another thread, but causes massive performance problems in Debug mode
                creationOptions: TaskCreationOptions.LongRunning,
                state: (nodes.ToList(), links.ToList())
            );

        private static PersonAndResShortestPaths FindPersonAndResShortestPaths(List<CosmicBody> nodes, List<Link> links)
            => new
            (
                PersonShortestPaths: FindShortestPaths
                (
                    nodes: nodes,
                    links: links,
                    distTimeCoeff: CurWorldConfig.personDistanceTimeCoeff,
                    distEnergyCoeff: CurWorldConfig.personDistanceEnergyCoeff
                ),
                ResShortestPaths: FindShortestPaths
                (
                    nodes: nodes,
                    links: links,
                    distTimeCoeff: CurWorldConfig.resDistanceTimeCoeff,
                    distEnergyCoeff: CurWorldConfig.resDistanceEnergyCoeff
                )
            );

        // currently uses Floyd-Warshall;
        // Dijkstra would be more efficient
        // TODO: implement Dijkstra and use pre-allocated buffers for computations to reduce GC pressure
        private static ShortestPaths FindShortestPaths(List<CosmicBody> nodes, List<Link> links, UDouble distTimeCoeff, UDouble distEnergyCoeff)
        {
            UDouble[,] distsArray = new UDouble[nodes.Count, nodes.Count];
            Link?[,] firstLinksArray = new Link[nodes.Count, nodes.Count];

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
                int i = nodes.IndexOf((CosmicBody)link.node1), j = nodes.IndexOf((CosmicBody)link.node2);
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

            Dictionary<(NodeID, NodeID), UDouble> distsDict = new();
            Dictionary<(NodeID, NodeID), Link?> firstLinksDict = new();
            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsDict.Add
                    (
                        key: (nodes[i].NodeID, nodes[j].NodeID),
                        value: distsArray[i, j]
                    );
                    firstLinksDict.Add
                    (
                        key: (nodes[i].NodeID, nodes[j].NodeID),
                        value: firstLinksArray[i, j]
                    );
                }

            return new(Dists: new(distsDict), FirstLinks: new(firstLinksDict));
        }

        public UDouble PersonDist(NodeID nodeID1, NodeID nodeID2)
            => personDists[(nodeID1, nodeID2)];

        public UDouble ResDist(NodeID nodeID1, NodeID nodeID2)
            => resDists[(nodeID1, nodeID2)];

        public MyVector2 NodePosition(NodeID nodeID)
            => nodeIDToNode[nodeID].Position;

        public void AddResDestinArrow(ResDestinArrow resDestinArrow)
        {
            resDestinArrow.activeChanged.Add(listener: this);
            resDestinArrows.AddChild(child: resDestinArrow);
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

        public void RemoveResDestinArrow(ResDestinArrow resDestinArrow)
        {
            resDestinArrow.activeChanged.Remove(listener: this);
            resDestinArrows.RemoveChild(child: resDestinArrow);
        }

        public void PreEnergyDistribUpdate()
        {
            foreach (var node in nodes)
                node.PreEnergyDistribUpdate();
        }

        public void Update(EnergyPile<HeatEnergy> vacuumHeatEnergyPile)
        {
            // TODO: improve performance of this
            if (shortestPathsTask.IsCompleted)
            {
                SetPersonAndResShortestPaths(shortestPathsTask.Result);
                SetShortestPathsTask();
            }

            CalcAndSetMaxLinkStats();
            links.ForEach(link => link.Update());
            foreach (var node in nodes)
                node.Update(personFirstLinks: personFirstLinks, vacuumHeatEnergyPile: vacuumHeatEnergyPile);

            links.ForEach(link => link.UpdatePeople());
            //nodes.ForEach(node => node.UpdatePeople());
            Stats = nodes.CombineRealPeopleStats().CombineWith(other: links.CombineRealPeopleStats());
            
            nodes.ForEach(node => node.StartSplitRes());
            foreach (var resOrRawMatsMix in CurResConfig.GetAllCurResOrRawMatsMix())
                SplitRes(resOrRawMatsMix: resOrRawMatsMix);
            foreach (var node in nodes)
                node.EndSplitRes(resFirstLinks: resFirstLinks);
        }

        public void UpdateHUDPos()
            => nodes.ForEach(node => node.UpdateHUDPos());

        /// <summary>
        /// TODO:
        /// choose random leafs
        /// </summary>
        public void SplitRes(ResOrRawMatsMix resOrRawMatsMix)
        {
            NodeInfo.Init(resOrRawMatsMix: resOrRawMatsMix);
            Dictionary<NodeID, NodeInfo> nodeInfos = nodes.ToDictionary
            (
                keySelector: node => node.NodeID,
                elementSelector: node => new NodeInfo(node: node)
            );

            foreach (var nodeInfo in nodeInfos.Values)
                foreach (var resDestin in nodeInfo.node.ResDestins(resOrRawMatsMix: resOrRawMatsMix))
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

            ulong MaxExtraRes(NodeID nodeID)
                => nodeInfos[nodeID].MaxExtraRes();

            while (sinks.Count > 0)
            {
                // want to choose random sink instead of this
                NodeInfo sink = sinks.Dequeue();
                sink.node.SplitRes
                (
                    nodeIDToNode: nodeID => nodeIDToNode[nodeID],
                    resOrRawMatsMix: resOrRawMatsMix,
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
                        nodeIDToNode: nodeID => nodeIDToNode[nodeID],
                        resOrRawMatsMix: resOrRawMatsMix,
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

        protected override void DrawChildren()
            => throw new InvalidOperationException();

        //void IChoiceChangedListener<IOverlay>.ChoiceChangedResponse(IOverlay prevOverlay)
        //{
        //    if (prevOverlay is IResource prevRes)
        //        RemoveChild(child: resDestinArrows.GetOrCreate(key: prevRes));

        //    if (CurWorldManager.Overlay is IResource resOrRawMatsMix)
        //        AddChild
        //        (
        //            child: resDestinArrows.GetOrCreate(key: resOrRawMatsMix),
        //            layer: CurWorldConfig.resDistribArrowsUILayer
        //        );
        //}

        void IActiveChangedListener.ActiveChangedResponse(WorldUIElement worldUIElement)
        {
            if (CurWorldManager.ArrowDrawingModeResOrRawMatsMix is not null)
            {
                if (worldUIElement.Active)
                {
                    var sourceNode = ActiveWorldElement as CosmicBody;
                    var destinationNode = worldUIElement as CosmicBody;
                    Debug.Assert(sourceNode is not null && destinationNode is not null);
                    sourceNode.AddResDestin(destinationId: destinationNode.NodeID, resOrRawMatsMix: CurWorldManager.ArrowDrawingModeResOrRawMatsMix.Value);
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
