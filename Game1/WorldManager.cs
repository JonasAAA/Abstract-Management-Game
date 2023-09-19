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
    public sealed class WorldManager
    {
        [Serializable]
        private sealed class PauseButtonTooltip : TextTooltipBase
        {
            protected sealed override string Text
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

        // Build helpers
        [Serializable]
        private sealed record BuildIndustryButtonClickedListener(Construction.GeneralParams ConstrGeneralParams) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => BuildingConfigPanelManager.StartBuildingConfig(constrGeneralParams: ConstrGeneralParams);
        }

        [Serializable]
        private sealed class BuildingConfigPanelManager : IItemChoiceSetter<MaterialPalette>, IItemChoiceSetter<ProductionChoice>
        {
            public static void StartBuildingConfig(Construction.GeneralParams constrGeneralParams)
            {
                CurWorldManager.activeUIManager.DisableAllUIElements();
                BuildingConfigPanelManager buildingConfigPanelManager = new(constrGeneralParams: constrGeneralParams);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: buildingConfigPanelManager.buildingConfigPanel,
                    position: new(HorizPosEnum.Right, VertPosEnum.Top)
                );
                foreach (var cosmicBodyBuildPanelManager in buildingConfigPanelManager.cosmicBodyBuildPanelManagers)
                    CurWorldManager.AddWorldHUDElement
                    (
                        worldHUDElement: cosmicBodyBuildPanelManager.CosmicBodyBuildPanel,
                        updateHUDPos: cosmicBodyBuildPanelManager.CosmicBodyPanelHUDPosUpdate
                    );
            }

            public ProductionChoice? ProductionChoice { get; private set; }

            private readonly Construction.GeneralParams constrGeneralParams;
            private readonly UIRectVertPanel<IHUDElement> buildingConfigPanel;
            private readonly List<CosmicBodyBuildPanelManager> cosmicBodyBuildPanelManagers;
            private readonly Dictionary<IProductClass, MaterialPalette> mutableBuildingMatPaletteChoices;
            private readonly Button cancelButton;

            private BuildingConfigPanelManager(Construction.GeneralParams constrGeneralParams)
            {
                this.constrGeneralParams = constrGeneralParams;
                mutableBuildingMatPaletteChoices = new();
                ProductionChoice = null;
                buildingConfigPanel = new(childHorizPos: HorizPosEnum.Left);
                buildingConfigPanel.AddChild(child: new TextBox() { Text = "Material Choices" });
                cancelButton = new
                (
                    shape: new MyRectangle(width: 100, height: 30),
                    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.CancelBuilding),
                    text: "Cancel",
                    color: colorConfig.deleteButtonColor
                );
                // Need to initialize all references so that when this gets copied, the fields are already initialized
                cosmicBodyBuildPanelManagers = new();

                EfficientReadOnlyHashSet<IProductClass> neededProductClasses = constrGeneralParams.neededProductClasses;
                Debug.Assert(neededProductClasses.Count is not 0);
                foreach (var productClass in neededProductClasses)
                {
                    UIRectHorizPanel<IHUDElement> matPaletteChoiceLine = new(childVertPos: VertPosEnum.Middle);
                    buildingConfigPanel.AddChild(child: matPaletteChoiceLine);
                    matPaletteChoiceLine.AddChild(child: new TextBox() { Text = $"{productClass} " });
                    Button startMatPaletteChoice = IndustryUIAlgos.CreateMatPaletteChoiceDropdown(matPaletteChoiceSetter: this, productClass: productClass);
                    matPaletteChoiceLine.AddChild(child: startMatPaletteChoice);
                }

                buildingConfigPanel.AddChild(child: new TextBox() { Text = "Production config" });

                var productionChoicePanel = constrGeneralParams.CreateProductionChoicePanel(productionChoiceSetter: this);
                if (productionChoicePanel is null)
                {
                    buildingConfigPanel.AddChild(child: new TextBox() { Text = UIAlgorithms.NothingToConfigure });
                    SetProductionChoice(productionChoice: new ProductionChoice(Choice: new UnitType()));
                }
                else
                    buildingConfigPanel.AddChild(child: productionChoicePanel);

                buildingConfigPanel.AddChild(child: cancelButton);
                cancelButton.clicked.Add(listener: new CancelBuildingButtonListener(BuildingConfigPanelManager: this));
                
                cosmicBodyBuildPanelManagers.AddRange
                (
                    collection: CurWorldManager.CurGraph.Nodes.Where
                    (
                        cosmicBody => !cosmicBody.HasIndustry
                    ).Select
                    (
                        cosmicBody =>
                        {
                            UIRectVertPanel<IHUDElement> cosmicBodyBuildPanel = new(childHorizPos: HorizPosEnum.Left);
                            cosmicBodyBuildPanel.AddChild(new TextBox() { Text = "Building Stats" });
                            Button buildButton = new
                            (
                                shape: new MyRectangle(width: 100, height: 30),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.BuildHereTooltip),
                                text: "Build here"
                            );
                            cosmicBodyBuildPanel.AddChild(child: buildButton);
                            buildButton.PersonallyEnabled = false;
                            buildButton.clicked.Add
                            (
                                listener: new BuildOnCosmicBodyButtonListener
                                (
                                    BuildingConfigPanelManager: this,
                                    CosmicBody: cosmicBody,
                                    ConstrGeneralParams: constrGeneralParams,
                                    NeededBuildingMatPaletteChoices: this.GetBuildingMatPaletteChoices()
                                )
                            );
                            return new CosmicBodyBuildPanelManager
                            (
                                CosmicBody: cosmicBody,
                                CosmicBodyBuildPanel: cosmicBodyBuildPanel,
                                CosmicBodyPanelHUDPosUpdate: new HUDElementPosUpdater
                                (
                                    HUDElement: cosmicBodyBuildPanel,
                                    baseWorldObject: cosmicBody,
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Middle),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                ),
                                BuildButton: buildButton
                            );
                        }
                    )
                );
            }

            private MaterialPaletteChoices GetBuildingMatPaletteChoices()
                => MaterialPaletteChoices.CreateOrThrow
                (
                    choices: new(dict: mutableBuildingMatPaletteChoices)
                );

            void IItemChoiceSetter<ProductionChoice>.SetChoice(ProductionChoice item)
                => SetProductionChoice(productionChoice: item);

            private void SetProductionChoice(ProductionChoice productionChoice)
            {
                ProductionChoice = productionChoice;
                EnableOrDisableBuildButtons();
            }

            void IItemChoiceSetter<MaterialPalette>.SetChoice(MaterialPalette item)
                => SetMatPaletteChoice(materialPalette: item);

            private void SetMatPaletteChoice(MaterialPalette materialPalette)
            {
                mutableBuildingMatPaletteChoices[materialPalette.productClass] = materialPalette;
#warning Complete this. In case all material choices are made, show player the stats of the to-be-constructed building
                EnableOrDisableBuildButtons();
            }

            private void EnableOrDisableBuildButtons()
            {
                bool enableBuildButtons = ProductionChoice is not null && constrGeneralParams.SufficientBuildingMatPalettes(curBuildingMatPaletteChoices: GetBuildingMatPaletteChoices());
                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers)
                    cosmicBodyPanelManager.BuildButton.PersonallyEnabled = enableBuildButtons;
            }

            public void StopBuildingConfig()
            {
                CurWorldManager.RemoveHUDElement(HUDElement: buildingConfigPanel);
                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: cosmicBodyPanelManager.CosmicBodyBuildPanel);
                CurWorldManager.activeUIManager.EnableAllUIElements();
            }
        }

        [Serializable]
        private sealed record CancelBuildingButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => BuildingConfigPanelManager.StopBuildingConfig();
        }

        [Serializable]
        private readonly record struct CosmicBodyBuildPanelManager(CosmicBody CosmicBody, UIRectVertPanel<IHUDElement> CosmicBodyBuildPanel, IAction CosmicBodyPanelHUDPosUpdate, Button BuildButton);

        [Serializable]
        private sealed record BuildOnCosmicBodyButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager, CosmicBody CosmicBody, Construction.GeneralParams ConstrGeneralParams, MaterialPaletteChoices NeededBuildingMatPaletteChoices) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                CosmicBody.StartConstruction
                (
                    constrConcreteParams: ConstrGeneralParams.CreateConcrete
                    (
                        nodeState: CosmicBody.NodeState,
                        neededBuildingMatPaletteChoices: NeededBuildingMatPaletteChoices,
                        productionChoice: BuildingConfigPanelManager.ProductionChoice ?? throw new InvalidOperationException()
                    )
                );
                BuildingConfigPanelManager.StopBuildingConfig();
            }
        }

        public static WorldManager CurWorldManager
            => Initialized ? curWorldManager : throw new InvalidOperationException($"must initialize {nameof(WorldManager)} first by calling {nameof(CreateWorldManager)} or {nameof(LoadWorldManager)}");

        [MemberNotNullWhen(returnValue: true, member: nameof(curWorldManager))]
        public static bool Initialized
            => curWorldManager is not null;

        public static WorldConfig CurWorldConfig { get; private set; } = null!;

        public static ResConfig CurResConfig { get; private set; } = null!;

        public static IndustryConfig CurIndustryConfig
            => CurWorldManager.industryConfig;

        private static WorldManager? curWorldManager;
        private static readonly EfficientReadOnlyCollection<Type> knownTypes = ComputeKnownTypes();

        private static EfficientReadOnlyCollection<Type> ComputeKnownTypes()
        {
            var nonSerializableTypes = GameMain.NonSerializableTypes();

            // TODO: move to a more appropriate class?
            HashSet<Type> knownTypesSet = new()
            {
                typeof(Dictionary<IndustryType, Score>),
                typeof(EfficientReadOnlyDictionary<NodeID, CosmicBody>),
                typeof(UIHorizTabPanel<IHUDElement>),
                typeof(UIHorizTabPanel<IHUDElement>.TabEnabledChangedListener),
                typeof(MultipleChoicePanel<string>),
                typeof(MultipleChoicePanel<string>.ChoiceEventListener),
                typeof(UIRectHorizPanel<IHUDElement>),
                typeof(UIRectHorizPanel<SelectButton>),
                typeof(UIRectVertPanel<IHUDElement>),
                //typeof(Counter<NumPeople>),
                typeof(EnergyCounter<HeatEnergy>),
                typeof(EnergyCounter<RadiantEnergy>),
                typeof(EnergyCounter<ElectricalEnergy>),
                //typeof(EnergyCounter<ResAmounts>),
            };
            knownTypesSet.UnionWith(Construction.GetKnownTypes());
            knownTypesSet.UnionWith(Manufacturing.GetKnownTypes());
            knownTypesSet.UnionWith(PowerPlant.GetKnownTypes());
            knownTypesSet.UnionWith(Mining.GetKnownTypes());
            knownTypesSet.UnionWith(MaterialProduction.GetKnownTypes());
            knownTypesSet.UnionWith(Storage.GetKnownTypes());
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
                    if (!nonSerializableTypes.Contains(type) && !(type.IsAbstract && type.IsSealed) && !type.IsInterface)
                    unserializedTypeList.Add(type);
            }
            if (unserializedTypeList.Count > 0)
                throw new InvalidStateException($"Every non-static, non-interface, non-enum type (except for types returned from NonSerializableTypes method(s)) must have attribute Serializable. The following types don't comply {unserializedTypeList.ToDebugString()}.");
            return knownTypesSet.ToEfficientReadOnlyCollection();
        }

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
            CurWorldManager.graph = Graph.CreateFromInfo
            (
                mapInfo: mapInfo,
                mapInfoCamera: mapInfoCamera,
                resConfig: curWorldManager.resConfig,
                industryConfig: curWorldManager.industryConfig
            );
            AddUIElements();
            CurWorldManager.Initialize();

            return;

            static void AddUIElements()
            {
                CurWorldManager.activeUIManager.AddWorldUIElement(UIElement: CurWorldManager.CurGraph);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.globalTextBox,
                    position: new(HorizPosEnum.Left, VertPosEnum.Top)
                );
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.pauseButton,
                    position: new(HorizPosEnum.Right, VertPosEnum.Bottom)
                );

                UIRectHorizPanel<IHUDElement> buildPanel = new(childVertPos: VertPosEnum.Middle);

                foreach (var constrGeneralParams in CurIndustryConfig.constrGeneralParamsList)
                {
                    Button buildIndustryButton = new
                    (
                        shape: new MyRectangle(width: 200, height: 30),
                        tooltip: constrGeneralParams.toopltip,
                        text: constrGeneralParams.buildButtonName
                    );
                    buildIndustryButton.clicked.Add(listener: new BuildIndustryButtonClickedListener(ConstrGeneralParams: constrGeneralParams));
#warning Complete this by adding how the button reacts to being pressed
                    buildPanel.AddChild(child: buildIndustryButton);
                }

                CurWorldManager.AddHUDElement(HUDElement: buildPanel, position: new(HorizPosEnum.Middle, VertPosEnum.Bottom));
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

                using var reader = XmlDictionaryReader.CreateBinaryReader
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

        public TimeSpan StartTime { get; }

        public TimeSpan CurTime { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public MyVector2 MouseWorldPos
            => worldCamera.ScreenPosToWorldPos(screenPos: (MyVector2)Mouse.GetState().Position);

        public TimeSpan MaxLinkTravelTime
            => CurGraph.MaxLinkTravelTime;

        public UDouble MaxLinkJoulesPerKg
            => CurGraph.MaxLinkJoulesPerKg;

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
        private readonly WorldCamera worldCamera;

        private Graph CurGraph
            => graph ?? throw new InvalidOperationException($"must initialize {nameof(graph)} first");
        private Graph? graph;

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

            globalTextBox = new(backgroundColor: colorConfig.UIBackgroundColor);
            // TODO: move these constants to a contants file
            globalTextBox.Shape.MinWidth = 300;

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
        }

        private void Initialize()
        {
            graph!.Initialize();
            lightManager.Initialize();
        }

        public void PublishMessage(IMessage message)
            // If exact same message already exists, don't add it a second time
            => throw new NotImplementedException();

        public UDouble PersonDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.PersonDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public UDouble ResDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.ResDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public MyVector2 NodePosition(NodeID nodeID)
            => CurGraph.NodePosition(nodeID: nodeID);

        public IEnumerable<IIndustry> SourcesOf(IResource resource)
            => CurGraph.SourcesOf(resource: resource);

        public IEnumerable<IIndustry> DestinsOf(IResource resource)
            => throw new NotImplementedException();

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

        public void AddHUDElement(IHUDElement? HUDElement, PosEnums position)
            => activeUIManager.AddHUDElement(HUDElement: HUDElement, position: position);

        public void SetHUDPopup(IHUDElement HUDElement, MyVector2 HUDPos, PosEnums origin)
            => activeUIManager.SetHUDPopup(HUDElement: HUDElement, HUDPos: HUDPos, origin: origin);

        public void AddWorldHUDElement(IHUDElement worldHUDElement, IAction updateHUDPos)
            => activeUIManager.AddWorldHUDElement(worldHUDElement: worldHUDElement, updateHUDPos: updateHUDPos);

        public void RemoveWorldHUDElement(IHUDElement worldHUDElement)
            => activeUIManager.RemoveWorldHUDElement(worldHUDElement: worldHUDElement);

        public void RemoveHUDElement(IHUDElement? HUDElement)
            => activeUIManager.RemoveHUDElement(HUDElement: HUDElement);

        public void EnableAllUIElements()
            => activeUIManager.EnableAllUIElements();

        public void DisableAllUIElements()
            => activeUIManager.DisableAllUIElements();

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
#warning Complete this. Do this speedup properly - have an initial simulation stage of the game (similar to Dwarf Fortress) when the speedup happens
            double speedup = (Mouse.GetState().MiddleButton == ButtonState.Pressed) ? 100 : 1;
            Elapsed = pauseButton.On ? TimeSpan.Zero : elapsedGameTime * CurWorldConfig.worldSecondsInGameSecond * speedup;
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
                globalTextBox.Text = energyManager.Summary().Trim();
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

            using var writer = XmlDictionaryWriter.CreateBinaryWriter(fileStream);
            serializer.WriteObject(writer, this);
        }
    }
}
