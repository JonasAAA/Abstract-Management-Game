using Game1.Delegates;
using Game1.Industries;
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
    public sealed class WorldManager : IDeletedListener, IClickedNowhereListener
    {
        [Serializable]
        private sealed class PauseButtonTooltip : TextTooltipBase
        {
            protected override string Text
                => onOffButton switch
                {
                    null => throw new InvalidOperationException($"Must initialize {nameof(PauseButtonTooltip)} by calling {nameof(Initialize)} first"),
                    not null => onOffButton.On switch
                    {
                        true => "Press to resume the game",
                        false => "Press to pause the game"
                    }
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
                Star[] stars = new Star[]
                {
                    new
                    (
                        state: new
                        (
                            starID: StarID.Create(),
                            position: new MyVector2(0, -300),
                            radius: 20,
                            prodWatts: 20000
                        ),
                        color: Color.Lerp(Color.White, Color.Red, .3f)
                    ),
                    new
                    (
                        state: new
                        (
                            starID: StarID.Create(),
                            position: new MyVector2(200, 300),
                            radius: 10,
                            prodWatts: 10000
                        ),
                        color: Color.Lerp(Color.White, Color.Blue, .3f)
                    ),
                    new
                    (
                        state: new
                        (
                            starID: StarID.Create(),
                            position: new MyVector2(-200, 100),
                            radius: 40,
                            prodWatts: 40000
                        ),
                        color: Color.Lerp(Color.White, new Color(0f, 1f, 0f), .3f)
                    ),
                };

                
                const int width = 8, height = 5, dist = 200;
                int startPlanetI = C.Random(min: 0, max: width),
                    startPlanetJ = C.Random(min: 0, max: height);
                Planet[,] nodes = new Planet[width, height];
                ResPile magicResPile = ResPile.CreateMagicUnlimitedPile();
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                    {
                        bool startPlanet = i == startPlanetI && j == startPlanetJ;
                        BasicResInd consistsOfResInd = BasicResInd.Random();
                        nodes[i, j] = new
                        (
                            state: new
                            (
                                nodeID: NodeID.Create(),
                                position: new MyVector2(i - (width - 1) * .5, j - (height - 1) * .5) * dist,
                                consistsOfResInd: consistsOfResInd,
                                mainResAmount: NodeState.ResAmountFromApproxRadius
                                (
                                    basicResInd: consistsOfResInd,
                                    approxRadius: MyMathHelper.Pow((UDouble)2, startPlanet ? 4.5 : C.Random(min: (double)3, max: 6))
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

                List<Link> links = new();
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height - 1; j++)
                        links.Add
                        (
                            item: new
                            (
                                node1: nodes[i, j],
                                node2: nodes[i, j + 1],
                                travelTime: TimeSpan.FromSeconds((i + 1) * distScale),
                                wattsPerKg: (UDouble)(j + 1.5) * distScale,
                                minSafeDist: CurWorldConfig.minSafeDist
                            )
                        );

                for (int i = 0; i < width - 1; i++)
                    for (int j = 0; j < height; j++)
                        links.Add
                        (
                            item: new
                            (
                                node1: nodes[i, j],
                                node2: nodes[i + 1, j],
                                travelTime: TimeSpan.FromSeconds((i + 1.5) * distScale),
                                wattsPerKg: (UDouble)(j + 1) * distScale,
                                minSafeDist: CurWorldConfig.minSafeDist
                            )
                        );

                return new
                (
                    stars: stars,
                    nodes: from Planet node in nodes
                           select node,
                    links: links
                );
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
            => @"C:\Users\Jonas\Desktop\Abstract Management Game\save.bin";

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
                typeof(Dictionary<IOverlay, IHUDElement>),
                typeof(ReadOnlyDictionary<NodeID, Planet>)
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

        public TimeSpan CurTime { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public MyVector2 MouseWorldPos
            => worldCamera.WorldPos(screenPos: (MyVector2)Mouse.GetState().Position);

        public TimeSpan MaxLinkTravelTime
            => CurGraph.maxLinkTravelTime;

        public UDouble MaxLinkJoulesPerKg
            => CurGraph.maxLinkJoulesPerKg;

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
                    if (CurGraph.ActiveWorldElement is Planet activeNode)
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
        private readonly MySet<Person> people;
        private readonly EnergyManager energyManager;
        private readonly ActivityManager activityManager;
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
            worldConfig = new();
            CurWorldConfig = worldConfig;
            resConfig = new();
            CurResConfig = resConfig;
            resConfig.Initialize();
            industryConfig = new();
            people = new();

            activityManager = new();
            energyManager = new();
            lightManager = new();

            worldCamera = new(startingWorldScale: worldConfig.startingWorldScale);

            activeUIManager = new();
            activeUIManager.clickedNowhere.Add(listener: this);

            globalTextBox = new(backgroundColor: curUIConfig.defaultUIBackgroundColor);
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

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
            => energyManager.AddEnergyConsumer(energyConsumer: energyConsumer);

        public void AddActivityCenter(IActivityCenter activityCenter)
            => activityManager.AddActivityCenter(activityCenter: activityCenter);

        public void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
            => lightManager.AddLightCatchingObject(lightCatchingObject: lightCatchingObject);

        public void AddLightSource(ILightSource lightSource)
            => lightManager.AddLightSource(lightSource: lightSource);

        public void AddPerson(Person person)
        {
            people.Add(person);
            person.Deleted.Add(listener: this);
        }

        public void Update(TimeSpan elapsed)
        {
            if (elapsed < TimeSpan.Zero)
                throw new ArgumentException();

            TimeSpan elapsedUITime = elapsed;

            if (pauseButton.On)
                elapsed = TimeSpan.Zero;

            Elapsed = elapsed;
            CurTime += Elapsed;

            worldCamera.Update(elapsed: elapsed, canScroll: CurGraph.MouseOn);

            lightManager.Update();

            energyManager.DistributeEnergy
            (
                nodeIDs: from node in CurGraph.Nodes
                         select node.NodeID,
                nodeIDToNode: nodeID => CurGraph.nodeIDToNode[nodeID]
            );

            CurGraph.Update();

            activityManager.ManageActivities(people: people);

            globalTextBox.Text = (energyManager.Summary() + $"population {people.Count}").Trim();

            activeUIManager.Update(elapsed: elapsedUITime);

            // THIS is a huge performance penalty
#if DEBUG
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

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is Person person)
                people.Remove(person);
            else
                throw new ArgumentException();
        }

        void IClickedNowhereListener.ClickedNowhereResponse()
            => ArrowDrawingModeOn = false;
    }
}
