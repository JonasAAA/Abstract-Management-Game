using Game1.Delegates;
using Game1.Industries;
using Game1.Inhabitants;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using static Game1.UI.ActiveUIManager;

namespace Game1
{
    [Serializable]
    public sealed class WorldManager : IClickedNowhereListener
    {
        [Serializable]
        private sealed class PauseButtonTooltip : TextTooltipBase
        {
            protected override string Text
                => onOffButton?.On switch
                {
                    true => "Press to resume the game",
                    false => "Press to pause the game",
                    null => throw new InvalidOperationException($"Must initialize {nameof(PauseButtonTooltip)} by calling {nameof(Initialize)} first")
                };

            private OnOffButton? onOffButton;

            public void Initialize(OnOffButton onOffButton)
                => this.onOffButton = onOffButton;
        }

        public static event Action? OnCreate;

        public static WorldManager CurWorldManager
            => Initialized ? curWorldManager : throw new InvalidOperationException($"must initialize {nameof(WorldManager)} first by calling {nameof(CreateWorldManager)} or {nameof(LoadWorldManager)}");

        public static IEvent<IChoiceChangedListener<IOverlay>> CurOverlayChanged
            => CurWorldManager.overlayChoicePanel.choiceChanged;

        [MemberNotNullWhen(returnValue: true, member: nameof(curWorldManager))]
        public static bool Initialized
            => curWorldManager is not null;

        public static WorldConfig CurWorldConfig { get; private set; }

        public static ResConfig CurResConfig { get; private set; }

        public static IndustryConfig CurIndustryConfig
            => CurWorldManager.industryConfig;

        public static bool SaveFileExists
            => File.Exists(GetSaveFilePath);

        private static WorldManager? curWorldManager;
        private static readonly Type[] knownTypes;

        public static ActiveUIManager CreateWorldManager()
        {
            curWorldManager = new();
            CurWorldManager.graph = CreateGraph();
            AddUIElements();
            CurWorldManager.Initialize();

            return CurWorldManager.activeUIManager;

            static void AddUIElements()
            {
                CurWorldManager.activeUIManager.AddNonHUDElement(UIElement: CurWorldManager.CurGraph, posTransformer: CurWorldManager.worldCamera);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.globalTextBox,
                    horizPos: HorizPos.Left,
                    vertPos: VertPos.Top
                );
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.overlayChoicePanel,
                    horizPos: HorizPos.Middle,
                    vertPos: VertPos.Top
                );
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.pauseButton,
                    horizPos: HorizPos.Right,
                    vertPos: VertPos.Bottom
                );
            }

            static Graph CreateGraph()
            {
                const int width = 10, height = 10, dist = 200, minAllowedDistToStar = 200;
                const double minPlanetRadiusExponet = 3, maxPlanetRadiusExponent = 7, startingPlanetExponent = 4.5;
                const double maxRandomPositionOffset = dist * .3;
                int startPlanetI = C.Random(min: width / 2 - 1, max: width / 2),
                    startPlanetJ = C.Random(min: height / 2 - 1, max: height / 2);
                CosmicBody?[,] cosmicBodies = new CosmicBody[width, height];
                ResPile magicResPile = ResPile.CreateByMagic(amount: ResAmounts.magicUnlimitedResAmounts);
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        bool startPlanet = i == startPlanetI && j == startPlanetJ;
                        BasicResInd consistsOfResInd = BasicResInd.Random();
                        MyVector2 position = new MyVector2(i - (width - 1) * .5, j - (height - 1) * .5) * dist + new MyVector2(C.Random(min: -1, max: 1), C.Random(min: -1, max: 1)) * maxRandomPositionOffset;
                        cosmicBodies[i, j] = new
                        (
                            state: new
                            (
                                position: position,
                                consistsOfResInd: consistsOfResInd,
                                mainResAmount: NodeState.ResAmountFromApproxRadius
                                (
                                    basicResInd: consistsOfResInd,
                                    approxRadius: MyMathHelper.Pow((UDouble)2, startPlanet ? startingPlanetExponent : C.Random(min: minPlanetRadiusExponet, max: maxPlanetRadiusExponent))
                                ),
                                resSource: magicResPile,
                                maxBatchDemResStored: 2
                            ),
                            activeColor: Color.White,
                            startingConditions: startPlanet switch
                            {
                                true =>
                                (
                                    houseFactory: CurIndustryConfig.basicHouseFactory,
                                    personCount: 20,
                                    resSource: magicResPile
                                ),
                                false => null
                            }
                        );
                    }

                UDouble distScale = (UDouble).1;
                Propor linkExistsProb = (Propor).7;

                List<Link> links = new();
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height - 1; j++)
                        AddLinkIfAppropriate(planet1: cosmicBodies[i, j], planet2: cosmicBodies[i, j + 1]);

                for (int i = 0; i < width - 1; i++)
                    for (int j = 0; j < height; j++)
                        AddLinkIfAppropriate(planet1: cosmicBodies[i, j], planet2: cosmicBodies[i + 1, j]);

                return new
                (
                    nodes: from CosmicBody cosmicBody in cosmicBodies
                           where cosmicBody is not null
                           select cosmicBody,
                    links: links
                );

                void AddLinkIfAppropriate(CosmicBody? planet1, CosmicBody? planet2)
                {
                    if (planet1 is not null && planet2 is not null && C.RandomBool(probOfTrue: linkExistsProb))
                        links.Add
                        (
                            item: new
                            (
                                node1: planet1,
                                node2: planet2,
                                minSafeDist: CurWorldConfig.minSafeDist
                            )
                        );
                }
            }
        }

        public static ActiveUIManager LoadWorldManager()
        {
            if (curWorldManager is not null || !SaveFileExists)
                throw new InvalidOperationException();

            curWorldManager = Deserialize();
            CurWorldConfig = curWorldManager.worldConfig;
            CurResConfig = curWorldManager.resConfig;
            CurWorldManager!.Initialize();

            return CurWorldManager.activeUIManager;

            static WorldManager Deserialize()
            {
                using FileStream fileStream = new(path: GetSaveFilePath, FileMode.Open, FileAccess.Read);
                DataContractSerializer serializer = GetDataContractSerializer();

                using XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader
                (
                    stream: fileStream,
                    quotas: new XmlDictionaryReaderQuotas()
                    {
                        MaxDepth = 1024
                    }
                );
                return (WorldManager)(serializer.ReadObject(reader, verifyObjectName: true) ?? throw new ArgumentNullException());
            }
        }

        // TODO: make this work not only on my machine
        private static string GetSaveFilePath
            => @"C:\Users\Jonas\Desktop\Serious\Game Projects\Abstract Management Game\save.bin";

        private static DataContractSerializer GetDataContractSerializer()
            => new
            (
                type: typeof(WorldManager),
                settings: new()
                {
                    KnownTypes = knownTypes,
                    PreserveObjectReferences = true,
                    SerializeReadOnlyTypes = true
                }
            );

        static WorldManager()
        {
            // TODO: move to a more appropriate class?
            HashSet<Type> knownTypesSet = new()
            {
                typeof(Dictionary<IndustryType, Score>),
                typeof(Dictionary<IOverlay, IHUDElement>),
                typeof(ReadOnlyDictionary<NodeID, CosmicBody>),
                typeof(UIHorizTabPanel<IHUDElement>),
                typeof(UIHorizTabPanel<IHUDElement>.TabEnabledChangedListener),
                typeof(MultipleChoicePanel<string>),
                typeof(MultipleChoicePanel<string>.ChoiceEventListener),
                typeof(MultipleChoicePanel<IOverlay>),
                typeof(MultipleChoicePanel<IOverlay>.ChoiceEventListener),
                typeof(UIRectHorizPanel<IHUDElement>),
                typeof(UIRectHorizPanel<SelectButton>),
                typeof(UIRectVertPanel<IHUDElement>),
                typeof(UITransparentPanel<ResDestinArrow>),
                //typeof(Counter<NumPeople>),
                typeof(EnergyCounter<HeatEnergy>),
                typeof(EnergyCounter<RadiantEnergy>),
                typeof(EnergyCounter<ElectricalEnergy>),
                //typeof(EnergyCounter<ResAmounts>),
            };
            List<Type> unserializedTypeList = new();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) is not null)
                    continue;

                if (Attribute.GetCustomAttribute(type, typeof(DataContractAttribute)) is not null
                    || Attribute.GetCustomAttribute(type, typeof(SerializableAttribute)) is not null
                    || type.IsEnum)
                    knownTypesSet.Add(type);
                else
                    if (type != typeof(Game1) && !(type.IsAbstract && type.IsSealed) && !type.IsInterface)
                    unserializedTypeList.Add(type);
            }
            if (unserializedTypeList.Count > 0)
                throw new Exception($"Every non-static, non-interface, non-enum type (except for Game1) must have attribute Serializable. The following types don't comply {unserializedTypeList.ToDebugString()}.");
            knownTypes = knownTypesSet.ToArray();

            CurWorldConfig = null!;
            CurResConfig = null!;
        }

        public IOverlay Overlay
            => overlayChoicePanel.SelectedChoiceLabel;

        public TimeSpan StartTime { get; }

        public TimeSpan CurTime { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public MyVector2 MouseWorldPos
            => worldCamera.WorldPos(screenPos: (MyVector2)Mouse.GetState().Position);

        public TimeSpan MaxLinkTravelTime
            => CurGraph.MaxLinkTravelTime;

        public UDouble MaxLinkJoulesPerKg
            => CurGraph.MaxLinkJoulesPerKg;

        public bool ArrowDrawingModeOn
        {
            get => arrowDrawingModeOn;
            set
            {
                if (arrowDrawingModeOn == value)
                    return;

                arrowDrawingModeOn = value;
                if (arrowDrawingModeOn)
                {
                    if (CurWorldManager.Overlay is not ResInd)
                        throw new Exception();
                    activeUIManager.DisableAllUIElements();
                    if (CurGraph.ActiveWorldElement is CosmicBody activeNode)
                    {
                        foreach (var node in CurGraph.Nodes)
                            if (activeNode.CanHaveDestin(destinationId: node.NodeID))
                                node.HasDisabledAncestor = false;
                    }
                    else
                        throw new Exception();
                }
                else
                    activeUIManager.EnableAllUIElements();
            }
        }

        private readonly WorldConfig worldConfig;
        private readonly ResConfig resConfig;
        private readonly IndustryConfig industryConfig;
        private readonly VirtualPeople people;
        private readonly ElectricalEnergyManager energyManager;
        private readonly ActivityManager activityManager;
        private readonly EnergyPile<HeatEnergy> vacuumHeatEnergyPile;
        private readonly LightManager lightManager;

        private readonly ActiveUIManager activeUIManager;
        private readonly TextBox globalTextBox;
        private readonly ToggleButton pauseButton;
        private readonly MultipleChoicePanel<IOverlay> overlayChoicePanel;
        private readonly WorldCamera worldCamera;

        private Graph CurGraph
            => graph ?? throw new InvalidOperationException($"must initialize {nameof(graph)} first");
        private Graph? graph;
        private bool arrowDrawingModeOn;

        private WorldManager()
        {
            StartTime = TimeSpan.Zero;
            CurTime = TimeSpan.Zero;
            worldConfig = new();
            CurWorldConfig = worldConfig;
            resConfig = new();
            CurResConfig = resConfig;
            resConfig.Initialize();
            industryConfig = new();
            people = new();

            activityManager = new();
            energyManager = new();
            vacuumHeatEnergyPile = EnergyPile<HeatEnergy>.CreateEmpty(locationCounters: LocationCounters.CreateEmpty());
            lightManager = new(vacuumHeatEnergyPile: vacuumHeatEnergyPile);

            worldCamera = new(startingWorldScale: worldConfig.startingWorldScale);

            activeUIManager = new();
            activeUIManager.clickedNowhere.Add(listener: this);

            globalTextBox = new(backgroundColor: colorConfig.UIBackgroundColor);
            // TODO: move these constants to a contants file
            globalTextBox.Shape.MinWidth = 300;

            // TODO: move these constants to a contants file
            overlayChoicePanel = new
            (
                horizontal: true,
                choiceWidth: 100,
                choiceHeight: 30,
                choiceLabelsAndTooltips:
                    from overlay in IOverlay.all
                    select
                    (
                        label: overlay,
                        tooltip: new ImmutableTextTooltip
                        (
                            text: overlay.SwitchExpression
                            (
                                singleResCase: resInd => $"Display information about resource {resInd}",
                                allResCase: () => "Display information about all resources",
                                powerCase: () => "Display information about power (production and consumption)",
                                peopleCase: () => "Display information about people"
                            )
                        ) as ITooltip
                    )
            );

            PauseButtonTooltip pauseButtonTooltip = new();
            pauseButton = new
            (
                shape: new MyRectangle
                (
                    width: 60,
                    height: 60
                ),
                tooltip: pauseButtonTooltip,
                on: false,
                text: "Toggle\nPause"
            );
            pauseButtonTooltip.Initialize(onOffButton: pauseButton);

            arrowDrawingModeOn = false;
        }

        private void Initialize()
        {
            graph!.Initialize();
            lightManager.Initialize();
            OnCreate?.Invoke();
        }

        public UDouble PersonDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.PersonDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public UDouble ResDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.ResDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public MyVector2 NodePosition(NodeID nodeID)
            => CurGraph.NodePosition(nodeID: nodeID);

        public void AddResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
            => CurGraph.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);

        public void RemoveResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
            => CurGraph.RemoveResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);

        public void AddHUDElement(IHUDElement? HUDElement, HorizPos horizPos, VertPos vertPos)
            => activeUIManager.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

        public void RemoveHUDElement(IHUDElement? HUDElement)
            => activeUIManager.RemoveHUDElement(HUDElement: HUDElement);

        public void AddEnergyProducer(IEnergyProducer energyProducer)
            => energyManager.AddEnergyProducer(energyProducer: energyProducer);

        public IEnergyDistributor EnergyDistributor
            => energyManager;

        public void AddActivityCenter(IActivityCenter activityCenter)
            => activityManager.AddActivityCenter(activityCenter: activityCenter);

        public void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
            => lightManager.AddLightCatchingObject(lightCatchingObject: lightCatchingObject);

        public void AddLightSource(ILightSource lightSource)
            => lightManager.AddLightSource(lightSource: lightSource);

        public void AddPerson(RealPerson realPerson)
            => people.Add(realPerson.asVirtual);

        public void Update(TimeSpan elapsedGameTime)
        {
            if (elapsedGameTime < TimeSpan.Zero)
                throw new ArgumentException();

            Elapsed = pauseButton.On ? TimeSpan.Zero : elapsedGameTime * CurWorldConfig.worldSecondsInGameSecond;
            CurTime += Elapsed;

            worldCamera.Update(elapsed: elapsedGameTime, canScroll: CurGraph.MouseOn);

            lightManager.Update();

            energyManager.DistributeEnergy
            (
                nodeIDs: from node in CurGraph.Nodes
                         select node.NodeID,
                nodeIDToNode: nodeID => CurGraph.nodeIDToNode[nodeID]
            );

            CurGraph.Update(vacuumHeatEnergyPile: vacuumHeatEnergyPile);

            activityManager.ManageActivities(people: people);

            Debug.Assert(people.Count == CurGraph.Stats.totalNumPeople);
            globalTextBox.Text = (energyManager.Summary() + CurGraph.Stats.ToString()).Trim();

            activeUIManager.Update(elapsed: elapsedGameTime);

            // THIS is a huge performance penalty
#if DEBUG2
            GC.Collect();
            GC.WaitForPendingFinalizers();
#endif
        }

        public void Draw()
        {
            worldCamera.BeginDraw();
            CurGraph.DrawBeforeLight();
            worldCamera.EndDraw();

            lightManager.Draw(worldToScreenTransform: worldCamera.GetToScreenTransform());

            worldCamera.BeginDraw();
            CurGraph.DrawAfterLight();
            worldCamera.EndDraw();

            activeUIManager.DrawHUD();
        }

        public void Save()
        {
            using FileStream fileStream = new(path: GetSaveFilePath, FileMode.Create);
            DataContractSerializer serializer = GetDataContractSerializer();

            using XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(fileStream);
            serializer.WriteObject(writer, this);
        }

        void IClickedNowhereListener.ClickedNowhereResponse()
            => ArrowDrawingModeOn = false;
    }
}
