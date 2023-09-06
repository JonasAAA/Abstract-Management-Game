﻿using Game1.Delegates;
using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using Game1.ContentHelpers;
using Game1.Collections;
using Game1.Industries;

namespace Game1
{
    [Serializable]
    public sealed class Graph : UIElement<IUIElement>, IActiveChangedListener, IWithRealPeopleStats
    {
        [Serializable]
        private readonly record struct ShortestPaths(EfficientReadOnlyDictionary<(NodeID, NodeID), UDouble> Dists, EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> FirstLinks);

        [Serializable]
        private readonly record struct PersonAndResShortestPaths(ShortestPaths PersonShortestPaths, ShortestPaths ResShortestPaths);

        public IEnumerable<CosmicBody> Nodes
            => nodes;

        public readonly EfficientReadOnlyDictionary<NodeID, CosmicBody> nodeIDToNode;
        public TimeSpan MaxLinkTravelTime { get; private set; }
        public UDouble MaxLinkJoulesPerKg { get; private set; }

        public sealed override bool CanBeClicked
            => true;
        public RealPeopleStats Stats { get; private set; }

        // THIS COLOR IS NOT USED
        protected sealed override Color Color
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
            }
        }

        private IEnumerable<IIndustry> Industries
        {
            get
            {
                foreach (var node in nodes)
                    if (node.Industry is not null)
                        yield return node.Industry;
            }
        }

        private readonly List<CosmicBody> nodes;
        private readonly List<Link> links;

        [NonSerialized] private Task<PersonAndResShortestPaths> shortestPathsTask;

        private WorldUIElement? activeWorldElement;

        public static Graph CreateFromInfo(FullValidMapInfo mapInfo, WorldCamera mapInfoCamera, ResConfig resConfig, IndustryConfig industryConfig)
        {
            RawMatAmounts startingRawMatTargetRatios = new
            (
                resAmounts: CurWorldConfig.startingRawMatTargetRatios.Select
                (
                    rawMatAmount => new ResAmount<RawMaterial>
                    (
                        res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: CurResConfig, ind: rawMatAmount.rawMatInd),
                        amount: rawMatAmount.amount
                    )
                )
            );
            ResPile magicUnlimitedStartingResPile = ResPile.CreateByMagic
            (
                amount: new
                (
                    resAmounts: resConfig.GetAllCurRes().Select
                    (
                        res => new ResAmount<IResource>
                        (
                            res: res,
                            amount: CurWorldConfig.magicUnlimitedStartingMaterialCount       
                        )
                    )
                ),
                temperature: CurWorldConfig.startingTemperature
            );
            Dictionary<string, CosmicBody> cosmicBodiesByName = mapInfo.CosmicBodies.ToDictionary
            (
                keySelector: cosmicBodyInfo => cosmicBodyInfo.Name,
                elementSelector: cosmicBodyInfo => new CosmicBody
                (
                    state: new
                    (
                        mapInfoCamera: mapInfoCamera,
                        cosmicBodyInfo: cosmicBodyInfo,
                        rawMatRatios: ResAndIndustryAlgos.CosmicBodyRandomRawMatRatios(startingRawMatTargetRatios: startingRawMatTargetRatios),
                        resSource: magicUnlimitedStartingResPile
                    ),
                    createIndustry: nodeState =>
                    {
                        var industry = CreateIndustry(nodeState: nodeState, cosmicBodyName: cosmicBodyInfo.Name);
                        Debug.Assert(!magicUnlimitedStartingResPile.IsEmpty);
                        return industry;
                    }
                )
            );
            return new
            (
                nodes: cosmicBodiesByName.Values.ToList(),
                links: mapInfo.Links.Select
                (
                    linkInfo => new Link
                    (
                        node1: cosmicBodiesByName[linkInfo.From],
                        node2: cosmicBodiesByName[linkInfo.To],
                        minSafeDist: CurWorldConfig.minSafeDist
                    )
                ).ToList()
            );

            IIndustry? CreateIndustry(IIndustryFacingNodeState nodeState, string cosmicBodyName)
            {
                if (cosmicBodyName == mapInfo.StartingInfo.PowerPlantCosmicBody)
                {
                    var concreteParams = industryConfig.startingPowerPlantParams.CreateConcrete
                    (
                        nodeState: nodeState,
                        neededBuildingMatChoices: resConfig.StartingMaterialChoices
                    ).UnwrapOrThrow();

                    var buildingResPile = ResPile.CreateIfHaveEnough
                    (
                        source: magicUnlimitedStartingResPile,
                        amount: concreteParams.BuildingCost
                    );
                    Debug.Assert(buildingResPile is not null);
                    return concreteParams.CreateIndustry
                    (
                        buildingResPile: buildingResPile
                    );
                }
                if (cosmicBodyName == mapInfo.StartingInfo.GearStorageCosmicBody)
                    return CreateStorageIndustry(productParamsName: "Gear");
                if (cosmicBodyName == mapInfo.StartingInfo.WireStorageCosmicBody)
                    return CreateStorageIndustry(productParamsName: "Wire");
                if (cosmicBodyName == mapInfo.StartingInfo.RoofTileStorageCosmicBody)
                    return CreateStorageIndustry(productParamsName: "Roof Tile");
                return null;

                IIndustry CreateStorageIndustry(string productParamsName)
                {
                    var concreteParams = industryConfig.startingStorageParams.CreateConcrete
                    (
                        nodeState: nodeState,
                        neededBuildingMatChoices: resConfig.StartingMaterialChoices
                    ).UnwrapOrThrow();

                    var buildingResPile = ResPile.CreateIfHaveEnough
                    (
                        source: magicUnlimitedStartingResPile,
                        amount: concreteParams.BuildingCost
                    );
                    Debug.Assert(buildingResPile is not null);
                    return concreteParams.CreateFullySpecifiedFilledStorage
                    (
                        buildingResPile: buildingResPile,
                        storedRes: Product.productParamsDict[productParamsName].GetProduct(materialChoices: resConfig.StartingMaterialChoices).UnwrapOrThrow(),
                        storedResSource: magicUnlimitedStartingResPile
                    );
                }
            }
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

            foreach (var worldUIElement in WorldUIElements)
                worldUIElement.activeChanged.Add(listener: this);

            activeWorldElement = null;
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

        public IEnumerable<IIndustry> SourcesOf(IResource resource)
            => Industries.Where(industry => industry.IsSourceOf(resource: resource));

        public IEnumerable<IIndustry> DestinsOf(IResource resource)
            => Industries.Where(industry => industry.IsDestinOf(resource: resource));

        public MyVector2 NodePosition(NodeID nodeID)
            => nodeIDToNode[nodeID].Position;

        public sealed override void OnClick()
        {
            base.OnClick();

            if (activeWorldElement is not null)
            {
                activeWorldElement.Active = false;
                activeWorldElement = null;
            }
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
            links.ForEach(link => link.StartUpdate());
            foreach (var node in nodes)
                node.StartUpdate(personFirstLinks: personFirstLinks, vacuumHeatEnergyPile: vacuumHeatEnergyPile);

            links.ForEach(link => link.EndUpdate());
            //nodes.ForEach(node => node.UpdatePeople());
            Stats = nodes.CombineRealPeopleStats().CombineWith(other: links.CombineRealPeopleStats());

            NewSplitRes();
            nodes.ForEach(node => node.EndUpdate(resFirstLinks: resFirstLinks));
        }

        public void NewSplitRes()
        {
            Dictionary<IResource, Dictionary<Algorithms.Vertex<IIndustry>, Algorithms.VertexInfo<IIndustry>>> resToRouteGraphs = new();
            foreach (var industry in Industries)
            {
                AllResAmounts demand = industry.GetDemand(), supply = industry.GetSupply();
                foreach (var res in industry.GetProducedRes())
                    resToRouteGraphs.GetOrCreate(key: res).Add
                    (
                        key: new(ResOwner: industry, IsSource: true),
                        value: new
                        (
                            directedNeighbours: industry.GetDestins(resource: res).ToList(),
                            amount: supply[res]
                        )
                    );
                foreach (var res in industry.GetConsumedRes())
                    resToRouteGraphs.GetOrCreate(key: res).Add
                    (
                        key: new(ResOwner: industry, IsSource: false),
                        value: new
                        (
                            directedNeighbours: industry.GetSources(resource: res).ToList(),
                            amount: demand[res]
                        )
                    );
            }

            foreach (var (res, routeGraph) in resToRouteGraphs)
            {
                EfficientReadOnlyCollection<Algorithms.ResPacket<IIndustry>> distribution = Algorithms.DistributeRes<IIndustry>(graph: new(dict: routeGraph));
                foreach (var (sourceIndustry, destinIndustry, amount) in distribution)
                {
                    if (amount is 0)
                        continue;
                    ResAmount<IResource> resAmount = new(res: res, amount: amount);
                    sourceIndustry.TransportResTo(destinIndustry: destinIndustry, resAmount: resAmount);
                    destinIndustry.WaitForResFrom(sourceIndustry: sourceIndustry, resAmount: resAmount);
                }
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

        protected sealed override void DrawChildren()
            => throw new InvalidOperationException();

        void IActiveChangedListener.ActiveChangedResponse(WorldUIElement worldUIElement)
        {
            if (worldUIElement.Active)
            {
                if (activeWorldElement is not null)
                    activeWorldElement.Active = false;
                activeWorldElement = worldUIElement;
            }
            else
            {
                if (activeWorldElement == worldUIElement)
                    activeWorldElement = null;
            }
        }
    }
}
