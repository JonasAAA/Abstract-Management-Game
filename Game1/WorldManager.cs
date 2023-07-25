using Game1.Collections;
using Game1.ContentHelpers;
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

        [Serializable]
        private readonly record struct BuildIndustryButtonClickedListener(Construction.GeneralParams ConstrGeneralParams) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => BuildingConfigPanelManager.StartBuildingConfig(constrGeneralParams: ConstrGeneralParams);
        }

        [Serializable]
        private readonly struct BuildingConfigPanelManager
        {
            public static void StartBuildingConfig(Construction.GeneralParams constrGeneralParams)
            {
                BuildingConfigPanelManager buildingConfigPanelManager = new(constrGeneralParams: constrGeneralParams);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: buildingConfigPanelManager.buildingConfigPanel,
                    horizPos: HorizPos.Right,
                    vertPos: VertPos.Top
                );
                foreach (var cosmicBodyPanelManager in buildingConfigPanelManager.cosmicBodyPanelManagers)
                    CurWorldManager.AddWorldHUDElement
                    (
                        worldHUDElement: cosmicBodyPanelManager.CosmicBodyPanel,
                        updateHUDPos: cosmicBodyPanelManager.CosmicBodyPanelHUDPosUpdate
                    );
            }

            private readonly Construction.GeneralParams constrGeneralParams;
            private readonly UIRectVertPanel<IHUDElement> buildingConfigPanel;
            private readonly List<CosmicBodyPanelManager> cosmicBodyPanelManagers;
            private readonly Dictionary<IMaterialPurpose, Material> buildingMaterialChoices;
            private readonly Button cancelButton;

            private BuildingConfigPanelManager(Construction.GeneralParams constrGeneralParams)
            {
                this.constrGeneralParams = constrGeneralParams;
                buildingMaterialChoices = new();
                buildingConfigPanel = new(childHorizPos: HorizPos.Left);
                buildingConfigPanel.AddChild(child: new TextBox() { Text = "Material Choices" });
                cancelButton = new
                (
                    shape: new MyRectangle(width: 100, height: 30),
                    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.CancelBuilding),
                    text: "Cancel",
                    color: colorConfig.deleteButtonColor
                );
                // Need to initialize all references so that when this gets copied, the fields are already initialized
                cosmicBodyPanelManagers = new();

                EfficientReadOnlyHashSet<IMaterialPurpose> neededMatPurposes = constrGeneralParams.neededMaterialPurposes;
                Debug.Assert(neededMatPurposes.Count is not 0);
                foreach (var materialPurpose in neededMatPurposes)
                {
                    UIRectHorizPanel<IHUDElement> materialChoiceLine = new(childVertPos: VertPos.Middle);
                    buildingConfigPanel.AddChild(child: materialChoiceLine);
                    materialChoiceLine.AddChild(child: new TextBox() { Text = materialPurpose.Name + " " });
                    Button startMaterialChoice = new
                    (
                        shape: new MyRectangle(width: 200, height: 30),
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoiceForPurposeTooltip(materialPurpose: materialPurpose)),
                        text: "+"
                    );
                    materialChoiceLine.AddChild(child: startMaterialChoice);
                    startMaterialChoice.clicked.Add
                    (
                        listener: new StartMaterialChoiceListener
                        (
                            BuildingConfigPanelManager: this,
                            StartMaterialChoice: startMaterialChoice,
                            MaterialPurpose: materialPurpose
                        )
                    );
                }

                buildingConfigPanel.AddChild(child: cancelButton);
                cancelButton.clicked.Add(listener: new CancelBuildingButtonListener(BuildingConfigPanelManager: this));
                
                var thisCopy = this;
                cosmicBodyPanelManagers.AddRange
                (
                    collection: CurWorldManager.CurGraph.Nodes.Where
                    (
                        cosmicBody => !cosmicBody.HasIndustry
                    ).Select
                    (
                        cosmicBody =>
                        {
                            UIRectVertPanel<IHUDElement> cosmicBodyPanel = new(childHorizPos: HorizPos.Left);
                            cosmicBodyPanel.AddChild(new TextBox() { Text = "Building Stats" });
                            Button buildButton = new
                            (
                                shape: new MyRectangle(width: 100, height: 30),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.BuildHereTooltip),
                                text: "Build here"
                            );
                            cosmicBodyPanel.AddChild(child: buildButton);
                            buildButton.PersonallyEnabled = false;
                            buildButton.clicked.Add
                            (
                                listener: new BuildOnCosmicBodyButtonListener
                                (
                                    BuildingConfigPanelManager: thisCopy,
                                    CosmicBody: cosmicBody,
                                    ConstrGeneralParams: constrGeneralParams,
                                    BuildingMaterialChoices: new(dict: thisCopy.buildingMaterialChoices)
                                )
                            );
                            return new CosmicBodyPanelManager
                            (
                                CosmicBody: cosmicBody,
                                CosmicBodyPanel: cosmicBodyPanel,
                                CosmicBodyPanelHUDPosUpdate: new CosmicBodyPanelHUDPosUpdater(CosmicBody: cosmicBody, CosmicBodyPanel: cosmicBodyPanel),
                                BuildButton: buildButton
                            );
                        }
                    )
                );
            }

            public void SetMatChoice(IMaterialPurpose materialPurpose, Material material)
            {
                buildingMaterialChoices[materialPurpose] = material;
                bool enableBuildButtons = constrGeneralParams.SufficientbuildingMaterials(curBuildingMaterialChoices: new(dict: buildingMaterialChoices));
                foreach (var cosmicBodyPanelManager in cosmicBodyPanelManagers)
                    cosmicBodyPanelManager.BuildButton.PersonallyEnabled = enableBuildButtons;
#warning Complete this. In case all material choices are made, show player the stats of the to-be-constructed building
            }

            public void RemoveFromActiveUIManager()
            {
                CurWorldManager.RemoveHUDElement(HUDElement: buildingConfigPanel);
                foreach (var cosmicBodyPanelManager in cosmicBodyPanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: cosmicBodyPanelManager.CosmicBodyPanel);
            }
        }

        [Serializable]
        private sealed record StartMaterialChoiceListener(BuildingConfigPanelManager BuildingConfigPanelManager, Button StartMaterialChoice, IMaterialPurpose MaterialPurpose) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                UIRectVertPanel<IHUDElement> materialChoicePopup = new(childHorizPos: Shapes.HorizPos.Middle);
                var popupHorizOrigin = HorizPos.Middle;
                var popupVertOrigin = VertPos.Middle;
                CurWorldManager.SetHUDPopup
                (
                    HUDElement: materialChoicePopup,
                    HUDPos: StartMaterialChoice.Shape.GetPosition
                    (
                        horizOrigin: popupHorizOrigin,
                        vertOrigin: popupVertOrigin
                    ),
                    horizOrigin: popupHorizOrigin,
                    vertOrigin: popupVertOrigin
                );
                foreach (var material in CurResConfig.GetCurRes<Material>())
                {
                    Button chooseMatButton = new
                    (
                        shape: new MyRectangle(width: 200, height: 30),
                        tooltip: new ImmutableTextTooltip(MaterialPurpose.TooltipTextFor(material: material)),
                        text: material.Name
                    );
                    materialChoicePopup.AddChild(child: chooseMatButton);
                    chooseMatButton.clicked.Add
                    (
                        listener: new MaterialChoiceListener
                        (
                            BuildingConfigPanelManager: BuildingConfigPanelManager,
                            StartMaterialChoice: StartMaterialChoice,
                            MaterialPurpose: MaterialPurpose,
                            Material: material
                        )
                    );
                }

            }
        }

        [Serializable]
        private sealed record MaterialChoiceListener(BuildingConfigPanelManager BuildingConfigPanelManager, Button StartMaterialChoice, IMaterialPurpose MaterialPurpose, Material Material) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                StartMaterialChoice.Text = Material.Name;
                BuildingConfigPanelManager.SetMatChoice(materialPurpose: MaterialPurpose, material: Material);
            }
        }

        [Serializable]
        private record CancelBuildingButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => BuildingConfigPanelManager.RemoveFromActiveUIManager();
        }

        [Serializable]
        private readonly record struct CosmicBodyPanelManager(CosmicBody CosmicBody, UIRectVertPanel<IHUDElement> CosmicBodyPanel, IAction CosmicBodyPanelHUDPosUpdate, Button BuildButton);

        [Serializable]
        private record CosmicBodyPanelHUDPosUpdater(CosmicBody CosmicBody, UIRectVertPanel<IHUDElement> CosmicBodyPanel) : IAction
        {
            void IAction.Invoke()
                => CosmicBodyPanel.Shape.SetPosition
                (
                    position: CurWorldManager.WorldPosToHUDPos(worldPos: CosmicBody.Position),
                    horizOrigin: HorizPos.Left,
                    vertOrigin: VertPos.Top
                );
        }

        [Serializable]
        private record BuildOnCosmicBodyButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager, CosmicBody CosmicBody, Construction.GeneralParams ConstrGeneralParams, MaterialChoices BuildingMaterialChoices) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => ConstrGeneralParams.CreateConcrete(nodeState: CosmicBody.NodeState, buildingMatChoices: BuildingMaterialChoices)
                    .ConvertMissingMatPurpsIntoError().SwitchStatement
                    (
                        ok: constrConcreteParams =>
                        {
                            CosmicBody.StartConstruction(constrConcreteParams: constrConcreteParams);
                            BuildingConfigPanelManager.RemoveFromActiveUIManager();
                        },
                        error: errors => throw new Exception(string.Join("\n", errors))
                    );
        }
        
        
        public static WorldManager CurWorldManager
            => Initialized ? curWorldManager : throw new InvalidOperationException($"must initialize {nameof(WorldManager)} first by calling {nameof(CreateWorldManager)} or {nameof(LoadWorldManager)}");

        //public static IEvent<IChoiceChangedListener<IOverlay>> CurOverlayChanged
        //    => CurWorldManager.overlayChoicePanel.choiceChanged;

        [MemberNotNullWhen(returnValue: true, member: nameof(curWorldManager))]
        public static bool Initialized
            => curWorldManager is not null;

        public static WorldConfig CurWorldConfig { get; private set; }

        public static ResConfig CurResConfig { get; private set; }

        public static IndustryConfig CurIndustryConfig
            => CurWorldManager.industryConfig;

        private static WorldManager? curWorldManager;
        private static readonly Type[] knownTypes;

        public static void CreateWorldManager(FullValidMapInfo mapInfo)
        {
            WorldCamera mapInfoCamera = new
            (
                worldCenter: mapInfo.StartingInfo.WorldCenter,
                startingWorldScale: WorldCamera.GetWorldScaleFromCameraViewHeight(cameraViewHeight: mapInfo.StartingInfo.CameraViewHeight),
                scrollSpeed: 1,
                screenBoundWidthForMapMoving: 1
            );
            curWorldManager = new();
            CurWorldManager.graph = Graph.CreateFromInfo(mapInfo: mapInfo, mapInfoCamera: mapInfoCamera);
            AddUIElements();
            CurWorldManager.Initialize();

            return;

            static void AddUIElements()
            {
                CurWorldManager.activeUIManager.AddWorldUIElement(UIElement: CurWorldManager.CurGraph);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.globalTextBox,
                    horizPos: HorizPos.Left,
                    vertPos: VertPos.Top
                );
                //CurWorldManager.AddHUDElement
                //(
                //    HUDElement: CurWorldManager.overlayChoicePanel,
                //    horizPos: HorizPos.Middle,
                //    vertPos: VertPos.Top
                //);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.pauseButton,
                    horizPos: HorizPos.Right,
                    vertPos: VertPos.Bottom
                );

                UIRectHorizPanel<IHUDElement> buildPanel = new(childVertPos: VertPos.Middle);

                foreach (var constrGeneralParams in CurIndustryConfig.constrGeneralParamsList)
                {
                    Button buildIndustryButton = new
                    (
                        shape: new Ellipse(width: 200, height: 20),
                        tooltip: constrGeneralParams.toopltip,
                        text: constrGeneralParams.name
                    );
                    buildIndustryButton.clicked.Add(listener: new BuildIndustryButtonClickedListener(ConstrGeneralParams: constrGeneralParams));
#warning Complete this by adding how the button reacts to being pressed
                    buildPanel.AddChild(child: buildIndustryButton);
                }

                UIHorizTabPanel<IHUDElement> globalTabs = new
                (
                    tabLabelWidth: 100,
                    tabLabelHeight: 30,
                    tabs: new List<(string tabLabelText, ITooltip tabTooltip, IHUDElement tab)>()
                    {
                        (
                            tabLabelText: "build",
                            tabTooltip: new ImmutableTextTooltip(text: UIAlgorithms.GlobalBuildTabTooltip),
                            tab: buildPanel
                        )
                    }
                );

                CurWorldManager.AddHUDElement(HUDElement: globalTabs, horizPos: HorizPos.Middle, vertPos: VertPos.Bottom);

                //buildButtonPannel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
                //UITabs.Add
                //((
                //    tabLabelText: "build",
                //    tabTooltip: new ImmutableTextTooltip(text: "Buildings/industries which could be built here"),
                //    tab: buildButtonPannel
                //));
                //foreach (var buildableParams in CurIndustryConfig.constrGeneralParamsList)
                //{
                //    Button buildIndustryButton = new
                //    (
                //        shape: new Ellipse
                //        (
                //            width: 200,
                //            height: 20
                //        ),
                //        tooltip: buildableParams.CreateTooltip(state: state),
                //        text: buildableParams.ButtonName
                //    );
                //    buildIndustryButton.clicked.Add
                //    (
                //        listener: new BuildIndustryButtonClickedListener
                //        (
                //            Node: this,
                //            BuildableParams: buildableParams
                //        )
                //    );

                //    buildButtonPannel.AddChild(child: buildIndustryButton);
                //}
            }
        }

        public static ActiveUIManager LoadWorldManager(string saveFilePath)
        {
            if (curWorldManager is not null)
                throw new InvalidOperationException();

            curWorldManager = Deserialize();
            CurWorldConfig = curWorldManager.worldConfig;
            CurResConfig = curWorldManager.resConfig;
            CurWorldManager!.Initialize();

            return CurWorldManager.activeUIManager;

            WorldManager Deserialize()
            {
                using FileStream fileStream = new(path: saveFilePath, FileMode.Open, FileAccess.Read);
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
                //typeof(MyDict<IOverlay, IHUDElement>),
                typeof(EfficientReadOnlyDictionary<NodeID, CosmicBody>),
                typeof(UIHorizTabPanel<IHUDElement>),
                typeof(UIHorizTabPanel<IHUDElement>.TabEnabledChangedListener),
                typeof(MultipleChoicePanel<string>),
                typeof(MultipleChoicePanel<string>.ChoiceEventListener),
                //typeof(MultipleChoicePanel<IOverlay>),
                //typeof(MultipleChoicePanel<IOverlay>.ChoiceEventListener),
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
            knownTypesSet.UnionWith(Construction.GetKnownTypes());
            knownTypesSet.UnionWith(Manufacturing.GetKnownTypes());
            knownTypesSet.UnionWith(Mining.GetKnownTypes());
            knownTypesSet.UnionWith(MaterialProduction.GetKnownTypes());
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

        //public IOverlay Overlay
        //    => overlayChoicePanel.SelectedChoiceLabel;

        public TimeSpan StartTime { get; }

        public TimeSpan CurTime { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public MyVector2 MouseWorldPos
            => worldCamera.ScreenPosToWorldPos(screenPos: (MyVector2)Mouse.GetState().Position);

        public TimeSpan MaxLinkTravelTime
            => CurGraph.MaxLinkTravelTime;

        public UDouble MaxLinkJoulesPerKg
            => CurGraph.MaxLinkJoulesPerKg;

        /// <summary>
        /// If null, then arrow drawing mode is not on
        /// </summary>
        public IResource? ArrowDrawingModeRes
        {
            get => arrowDrawingModeRes;
            set
            {
                if (arrowDrawingModeRes == value)
                    return;

                arrowDrawingModeRes = value;
                if (arrowDrawingModeRes is not null)
                {
                    activeUIManager.DisableAllUIElements();
                    if (CurGraph.ActiveWorldElement is CosmicBody activeNode)
                    {
                        foreach (var node in CurGraph.Nodes)
                            if (activeNode.CanHaveDestin(destinationId: node.NodeID, res: arrowDrawingModeRes))
                                node.HasDisabledAncestor = false;
                    }
                    else
                        throw new Exception();
                }
                else
                    activeUIManager.EnableAllUIElements();
            }
        }

        //public bool ArrowDrawingModeOn
        //{
        //    get => arrowDrawingModeOn;
        //    set
        //    {
        //        if (arrowDrawingModeOn == value)
        //            return;

        //        arrowDrawingModeOn = value;
        //        if (arrowDrawingModeOn)
        //        {
        //            if (CurWorldManager.Overlay is not IResource)
        //                throw new Exception();
        //            activeUIManager.DisableAllUIElements();
        //            if (CurGraph.ActiveWorldElement is CosmicBody activeNode)
        //            {
        //                foreach (var node in CurGraph.Nodes)
        //                    if (activeNode.CanHaveDestin(destinationId: node.NodeID))
        //                        node.HasDisabledAncestor = false;
        //            }
        //            else
        //                throw new Exception();
        //        }
        //        else
        //            activeUIManager.EnableAllUIElements();
        //    }
        //}

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
        //private readonly MultipleChoicePanel<IOverlay> overlayChoicePanel;
        private readonly WorldCamera worldCamera;

        private Graph CurGraph
            => graph ?? throw new InvalidOperationException($"must initialize {nameof(graph)} first");
        private Graph? graph;
        private IResource? arrowDrawingModeRes;
        //private bool arrowDrawingModeOn;

        private WorldManager()
        {
            StartTime = TimeSpan.Zero;
            CurTime = TimeSpan.Zero;
            worldConfig = new();
            CurWorldConfig = worldConfig;
            CurResConfig = resConfig = new();
            CurResConfig.Initialize();
            industryConfig = new();
            people = new();

            activityManager = new();
            energyManager = new();
            vacuumHeatEnergyPile = EnergyPile<HeatEnergy>.CreateEmpty(locationCounters: LocationCounters.CreateEmpty());
            lightManager = new(vacuumHeatEnergyPile: vacuumHeatEnergyPile);

            worldCamera = new
            (
                worldCenter: MyVector2.zero,
                startingWorldScale: 1 / worldConfig.metersPerStartingPixel,
                scrollSpeed: worldConfig.scrollSpeed,
                screenBoundWidthForMapMoving: worldConfig.screenBoundWidthForMapMoving
            );

            activeUIManager = new(worldCamera: worldCamera);
            activeUIManager.clickedNowhere.Add(listener: this);

            globalTextBox = new(backgroundColor: colorConfig.UIBackgroundColor);
            // TODO: move these constants to a contants file
            globalTextBox.Shape.MinWidth = 300;

            //// TODO: move these constants to a contants file
            //overlayChoicePanel = new
            //(
            //    horizontal: true,
            //    choiceWidth: 100,
            //    choiceHeight: 30,
            //    choiceLabelsAndTooltips:
            //        from overlay in IOverlay.all
            //        select
            //        (
            //            label: overlay,
            //            tooltip: new ImmutableTextTooltip
            //            (
            //                text: overlay.SwitchExpression
            //                (
            //                    singleResCase: res => $"Display information about resource {res}",
            //                    allResCase: () => "Display information about all resources",
            //                    powerCase: () => "Display information about power (production and consumption)",
            //                    peopleCase: () => "Display information about people"
            //                )
            //            ) as ITooltip
            //        )
            //);

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

            arrowDrawingModeRes = null;
        }

        private void Initialize()
        {
            graph!.Initialize();
            lightManager.Initialize();
        }

        public void PublishMessage(IMessage message)
        {
            // If exact same message already exists, don't add it a second time
            throw new NotImplementedException();
        }

        public UDouble PersonDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.PersonDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public UDouble ResDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.ResDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public MyVector2 NodePosition(NodeID nodeID)
            => CurGraph.NodePosition(nodeID: nodeID);

        public void AddResDestinArrow(ResDestinArrow resDestinArrow)
            => CurGraph.AddResDestinArrow(resDestinArrow: resDestinArrow);

        public void RemoveResDestinArrow(ResDestinArrow resDestinArrow)
            => CurGraph.RemoveResDestinArrow(resDestinArrow: resDestinArrow);

        public MyVector2 ScreenPosToWorldPos(MyVector2 screenPos)
            => worldCamera.ScreenPosToWorldPos(screenPos: screenPos);

        public UDouble ScreenLengthToWorldLength(UDouble screenLength)
            => worldCamera.ScreenLengthToWorldLength(screenLength: screenLength);

        public MyVector2 WorldPosToHUDPos(MyVector2 worldPos)
            => ScreenPosToHUDPos(screenPos: worldCamera.WorldPosToScreenPos(worldPos: worldPos));

        //public MyVector2 HUDPosToWorldPos(MyVector2 HUDPos)
        //    => worldCamera.ScreenPosToWorldPos(screenPos: HUDPosToScreenPos(HUDPos: HUDPos));

        //public UDouble HUDLengthToWorldLength(UDouble HUDLength)
        //    => worldCamera.ScreenLengthToWorldLength
        //    (
        //        screenLength: HUDLengthToScreenLength(HUDLength: HUDLength)
        //    );

        public void AddHUDElement(IHUDElement? HUDElement, HorizPos horizPos, VertPos vertPos)
            => activeUIManager.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

        public void SetHUDPopup(IHUDElement? HUDElement, MyVector2 HUDPos, HorizPos horizOrigin, VertPos vertOrigin)
            => activeUIManager.SetHUDPopup(HUDElement: HUDElement, HUDPos: HUDPos, horizOrigin: horizOrigin, vertOrigin: vertOrigin);

        public void AddWorldHUDElement(IHUDElement worldHUDElement, IAction updateHUDPos)
            => activeUIManager.AddWorldHUDElement(worldHUDElement: worldHUDElement, updateHUDPos: updateHUDPos);

        public void RemoveWorldHUDElement(IHUDElement worldHUDElement)
            => activeUIManager.RemoveWorldHUDElement(worldHUDElement: worldHUDElement);

        public void RemoveHUDElement(IHUDElement? HUDElement)
            => activeUIManager.RemoveHUDElement(HUDElement: HUDElement);

        public void AddEnergyProducer(IEnergyProducer energyProducer)
            => energyManager.AddEnergyProducer(energyProducer: energyProducer);

        public void RemoveEnergyProducer(IEnergyProducer energyProducer)
            => energyManager.RemoveEnergyProducer(energyProducer: energyProducer);

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

            if (Elapsed > TimeSpan.Zero)
            {
                lightManager.Update();

                CurGraph.PreEnergyDistribUpdate();

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
            }
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

        public void Save(string saveFilePath)
        {
            using FileStream fileStream = new(path: saveFilePath, FileMode.Create);
            DataContractSerializer serializer = GetDataContractSerializer();

            using XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(fileStream);
            serializer.WriteObject(writer, this);
        }

        void IClickedNowhereListener.ClickedNowhereResponse()
            => arrowDrawingModeRes = null;
    }
}
