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
using Game1.Industries;
using System.Linq;

namespace Game1
{
    [Serializable]
    public sealed class Graph : UIElement<IUIElement>, IActiveChangedListener, IWithRealPeopleStats
    {
        [Serializable]
        private readonly record struct ShortestPaths(EfficientReadOnlyDictionary<(NodeID, NodeID), UDouble> Dists, EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> FirstLinks);

        [Serializable]
        private readonly record struct PersonAndResShortestPaths(ShortestPaths PersonShortestPaths, ShortestPaths ResShortestPaths);

        public EfficientReadOnlyCollection<CosmicBody> Nodes
            => new(list: nodes);

        public readonly EfficientReadOnlyDictionary<NodeID, CosmicBody> nodeIDToNode;
        public TimeSpan MaxLinkTravelTime { get; private set; }
        public UDouble MaxLinkJoulesPerKg { get; private set; }

        public sealed override bool CanBeClicked
            => false;
        public RealPeopleStats Stats { get; private set; }
        public Temperature AverageTemperature { get; private set; }

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

        private IEnumerable<IWorldObject> WorldObjects
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

        private WorldUIElement? ActiveWorldElement
        {
            get => activeWorldElement;
            set
            {
                if (activeWorldElement == value)
                    return;
                if (activeWorldElement is not null)
                    activeWorldElement.Active = false;
                activeWorldElement = value;
                if (activeWorldElement is not null)
                    activeWorldElement.Active = true;
            }
        }

        /// <summary>
        /// NEVER use this directly. Use the associated property instead
        /// </summary>
        private WorldUIElement? activeWorldElement;

        public static Graph CreateFromInfo(FullValidMapInfo mapInfo, WorldCamera mapInfoCamera, ResConfig resConfig, IndustryConfig industryConfig)
        {
            //RawMatAmounts startingRawMatTargetRatios = new
            //(
            //    resAmounts: CurWorldConfig.startingRawMatTargetRatios.Select
            //    (
            //        rawMatAmount => new ResAmount<RawMaterial>
            //        (
            //            res: CurResConfig.GetRawMatFromID(rawMatID: rawMatAmount.rawMatID),
            //            amount: rawMatAmount.amount
            //        )
            //    )
            //);
            var magicUnlimitedStartingResPile = ResPile.CreateByMagic
            (
                amount: new
                (
                    resAmounts: resConfig.AllCurRes.Select
                    (
                        res => new ResAmount<IResource>
                        (
                            res: res,
                            amount: CurWorldConfig.magicUnlimitedStartingMaterialCount / res.Area.valueInMetSq
                        )
                    )
                ),
                temperature: CurWorldConfig.startingTemperature
            );
            var cosmicBodiesByName = mapInfo.CosmicBodies.ToDictionary
            (
                keySelector: cosmicBodyInfo => cosmicBodyInfo.Name,
                elementSelector: cosmicBodyInfo => new CosmicBody
                (
                    state: new
                    (
                        mapInfoCamera: mapInfoCamera,
                        cosmicBodyInfo: cosmicBodyInfo,
                        rawMatRatios: new
                        (
                            resAmounts: cosmicBodyInfo.Composition.Select
                            (
                                rawMatPropor => new ResAmount<RawMaterial>
                                (
                                    res: CurResConfig.GetRawMatFromID(rawMatID: rawMatPropor.RawMaterial),
                                    amount: rawMatPropor.Percentage
                                )
                            )
                        ),
                        resSource: magicUnlimitedStartingResPile
                    ),
                    createIndustry: nodeState =>
                    {
                        foreach (var (startingBuilding, cosmicBodyName) in mapInfo.StartingInfo.StartingBuildingToCosmicBody)
                            if (cosmicBodyName == cosmicBodyInfo.Name)
                            {
                                var industry = CreateIndustry(nodeState: nodeState, startingBuilding: startingBuilding);
                                Debug.Assert(!magicUnlimitedStartingResPile.IsEmpty);
                                return industry;
                            }
                        return null;
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

            IIndustry CreateIndustry(IIndustryFacingNodeState nodeState, StartingBuilding startingBuilding)
            {
                return startingBuilding switch
                {
                    StartingBuilding.PowerPlant => CreatePowerPlantIndustry(),
                    StartingBuilding.GearStorage => CreateStorageIndustry(productParamsName: "Gear"),
                    StartingBuilding.WireStorage => CreateStorageIndustry(productParamsName: "Wire"),
                };

                IIndustry CreatePowerPlantIndustry()
                {
                    var concreteParams = industryConfig.startingPowerPlantParams.CreateConcrete
                    (
                        nodeState: nodeState,
                        neededBuildingMatPaletteChoices: resConfig.StartingMaterialPaletteChoices.FilterOutUnneededMatPalettes
                        (
                            neededProductClasses: industryConfig.startingPowerPlantParams.BuildingCostPropors.neededProductClasses
                        )
                    );

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

                IIndustry CreateStorageIndustry(string productParamsName)
                {
                    var productParams = Product.productParamsDict[productParamsName];
                    var concreteParams = industryConfig.startingStorageParams.CreateConcrete
                    (
                        nodeState: nodeState,
                        neededBuildingMatPaletteChoices: resConfig.StartingMaterialPaletteChoices.FilterOutUnneededMatPalettes
                        (
                            neededProductClasses: industryConfig.startingStorageParams.BuildingCostPropors.neededProductClasses
                        ),
                        storageChoice: productParams.GetProduct(materialPalette: resConfig.StartingMaterialPaletteChoices[productParams.productClass])
                    );

                    var buildingResPile = ResPile.CreateIfHaveEnough
                    (
                        source: magicUnlimitedStartingResPile,
                        amount: concreteParams.BuildingCost
                    );
                    Debug.Assert(buildingResPile is not null);
                    return concreteParams.CreateFilledStorage
                    (
                        buildingResPile: buildingResPile,
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

            DeactivateWorldElements();
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

        public void DeactivateWorldElements()
            => ActiveWorldElement = null;

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
            var distsArray = new UDouble[nodes.Count, nodes.Count];
            var firstLinksArray = new Link?[nodes.Count, nodes.Count];

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

            Dictionary<(NodeID, NodeID), UDouble> distsDict = [];
            Dictionary<(NodeID, NodeID), Link?> firstLinksDict = [];
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

        public IEnumerable<IIndustry> IndustriesWithPossibleNeighbourhood(NeighborDir neighborDir, IResource resource)
            => Industries.Where(industry => industry.IsNeighborhoodPossible(neighborDir: neighborDir, resource: resource));

        public IEnumerable<CosmicBody> CosmicBodies()
            => nodeIDToNode.Values;

        public MyVector2 NodePosition(NodeID nodeID)
            => nodeIDToNode[nodeID].Position;

        public sealed override void OnClick()
        {
            base.OnClick();

            DeactivateWorldElements();
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

            DistributeRes();
            nodes.ForEach(node => node.EndUpdate(resFirstLinks: resFirstLinks));

            AverageTemperature = ResAndIndustryAlgos.CalculateTemperature
            (
                heatEnergy: WorldObjects.Sum(worldObject => worldObject.HeatEnergy),
                heatCapacity: WorldObjects.Sum(worldObject => worldObject.HeatCapacity)
            );
        }

        public void DistributeRes()
        {
            Dictionary<IResource, Dictionary<Algorithms.Vertex<IIndustry>, Algorithms.VertexInfo<IIndustry>>> resToRouteGraphs = [];
            foreach (var industry in Industries)
            {
                foreach (var neighborDir in Enum.GetValues<NeighborDir>())
                {
                    AllResAmounts requestedResAmounts = industry.GetResAmountsRequestToNeighbors(neighborDir: neighborDir);
                    foreach (var res in industry.GetResWithPotentialNeighborhood(neighborDir: neighborDir))
                        resToRouteGraphs.GetOrCreate(key: res).Add
                        (
                            key: new(ResOwner: industry, IsSource: neighborDir == NeighborDir.Out),
                            value: new
                            (
                                directedNeighbours: industry.GetResNeighbors(neighborDir: neighborDir, resource: res).ToList(),
                                amount: requestedResAmounts[res]
                            )
                        );
                }
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
                ActiveWorldElement = worldUIElement;
            else
            {
                Debug.Assert(ActiveWorldElement == worldUIElement);
                DeactivateWorldElements();
            }
        }
    }
}
