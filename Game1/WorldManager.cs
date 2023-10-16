﻿using Game1.Collections;
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
            knownTypesSet.UnionWith(Landfill.GetKnownTypes());
            knownTypesSet.UnionWith(MaterialProduction.GetKnownTypes());
            knownTypesSet.UnionWith(Storage.GetKnownTypes());
            knownTypesSet.UnionWith(Dropdown.GetKnownTypes());
            knownTypesSet.UnionWith(IndustryUIAlgos.GetKnownTypes());
            knownTypesSet.UnionWith(FunctionGraphImage.GetKnownTypes());
            knownTypesSet.UnionWith(FunctionGraphWithHighlighImage.GetKnownTypes());

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
                //CurWorldManager.AddHUDElement
                //(
                //    HUDElement: CurWorldManager.graphTrials,
                //    position: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                //);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: CurWorldManager.pauseButton,
                    position: new(HorizPosEnum.Right, VertPosEnum.Bottom)
                );

                UIRectVertPanel<IHUDElement> buildPanel = new
                (
                    childHorizPos: HorizPosEnum.Middle,
                    children: CurIndustryConfig.constrGeneralParamsList.Select
                    (
                        constrGeneralParams =>
                        {
                            Button buildIndustryButton = new
                            (
                                shape: new MyRectangle(width: curUIConfig.wideUIElementWidth, height: curUIConfig.UILineHeight),
                                tooltip: constrGeneralParams.toopltip,
                                text: constrGeneralParams.buildButtonName
                            );
                            buildIndustryButton.clicked.Add
                            (
                                listener: new BuildIndustryButtonClickedListener
                                (
                                    CosmicBodies: CurWorldManager.CurGraph.Nodes,
                                    ConstrGeneralParams: constrGeneralParams
                                )
                            );
#warning Complete this by adding how the button reacts to being pressed
                            return buildIndustryButton;
                        }
                    )
                );

                CurWorldManager.AddHUDElement(HUDElement: buildPanel, position: new(HorizPosEnum.Right, VertPosEnum.Top));
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

        public readonly IMaterialPurpose.Options matPurposeOptions;

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
        private readonly UIRectHorizPanel<IHUDElement> graphTrials;
        private readonly ToggleButton pauseButton;
        private readonly WorldCamera worldCamera;

        private Graph CurGraph
            => graph ?? throw new InvalidOperationException($"must initialize {nameof(graph)} first");
        private Graph? graph;

        private WorldManager()
        {
            StartTime = TimeSpan.Zero;
            CurTime = TimeSpan.Zero;
            matPurposeOptions = new();
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

#warning delete this
            graphTrials = new
            (
                childVertPos: VertPosEnum.Top,
                children: new List<IHUDElement>()
                {
                    GenerateGraphMeanPanel(exponent: double.NegativeInfinity),
                    GenerateGraphMeanPanel(exponent: -4),
                    GenerateGraphMeanPanel(exponent: -1),
                    GenerateGraphMeanPanel(exponent: 0),
                    GenerateGraphMeanPanel(exponent: 1),
                    GenerateGraphMeanPanel(exponent: 4),
                    GenerateGraphMeanPanel(exponent: double.PositiveInfinity)
                }
            );
            //graphTrials.AddChild(child: GenerateGraphCompositionPanel(modifyRes: (x, cnt) => x / cnt,  operation: (x, y) => x + y, functionName: "sum"));
            //graphTrials.AddChild(child: GenerateGraphCompositionPanel(modifyRes: (x, cnt) => x, operation: (x, y) => x * y, functionName: "product"));
            //graphTrials.AddChild(child: GenerateGraphCompositionPanel(modifyRes: (x, cnt) => x, operation: (x, y) => MyMathHelper.Min(x, y), functionName: "minimum"));

            PauseButtonTooltip pauseButtonTooltip = new();
            pauseButton = new
            (
                shape: new MyRectangle
                (
                    width: 2 * curUIConfig.UILineHeight,
                    height: 2 * curUIConfig.UILineHeight
                ),
                tooltip: pauseButtonTooltip,
                on: false,
                text: "Toggle\nPause"
            );
            pauseButtonTooltip.Initialize(onOffButton: pauseButton);


            UIRectVertPanel<IHUDElement> GenerateGraphMeanPanel(double exponent)
            {
                const ulong minX = 1, maxX = 10;

                List<(UDouble weight, Func<UDouble, Propor> func)> functions = new()
                {
                    (weight: 1, func: x => Algorithms.Normalize(value: MyMathHelper.Sin(x), start: -1, stop: 1)),
                    (weight: 10, func: x => Algorithms.Normalize(value: x, start: minX, stop: maxX)),
                    (weight: 3, func: x => Algorithms.Normalize(value: -(double)x * x, start: MyMathHelper.Min(-(double)minX * minX, -(double)maxX * maxX), stop: 0))
                };

                UDouble totalWeight = functions.Sum(weightAndFunc => weightAndFunc.weight);

                Func<UDouble, Propor> compositionFunc = x => Propor.PowerMean
                (
                    args: functions.Select(args => (weight: (Propor)(args.weight / totalWeight), value: args.func(x))),
                    exponent: exponent
                );

                string powerStr = exponent switch
                {
                    double.NegativeInfinity => "-inf",
                    double.PositiveInfinity => "+inf",
                    double pow => pow.ToString(),
                };

                return new UIRectVertPanel<IHUDElement>
                (
                    childHorizPos: HorizPosEnum.Middle,
                    children: new List<IHUDElement>()
                    {
                        new TextBox(text: $"mean of deg {powerStr}")
                    }.Concat
                    (
                        functions.Select(args => CreateFunctionGraph(weight: (Propor)(args.weight / totalWeight), func: args.func))
                    ).Append
                    (
                        new TextBox(text: "=")
                    ).Append
                    (
                        CreateFunctionGraph(weight: (Propor)1, func: compositionFunc)
                    )
                );

                IHUDElement CreateFunctionGraph(Propor weight, Func<UDouble, Propor> func)
                    => new UIRectHorizPanel<IHUDElement>
                    (
                        childVertPos: VertPosEnum.Bottom,
                        children: new List<IHUDElement>()
                        {
                            new VertProporBar
                            (
                                width: 10,
                                height: curUIConfig.UILineHeight,
                                propor: weight,
                                barColor: Color.Green,
                                backgroundColor: colorConfig.UIBackgroundColor
                            ),
                            new ImageHUDElement
                            (
                                image: new FunctionGraphImage<UDouble, Propor>
                                (
                                    width: curUIConfig.standardUIElementWidth,
                                    height: curUIConfig.UILineHeight,
                                    backgroundColor: Color.Yellow,
                                    lineColor: Color.Red,
                                    lineWidth: 1,
                                    minX: minX,
                                    maxX: maxX,
                                    minY: Propor.empty,
                                    maxY: Propor.full,
                                    numXSamples: 1000,
                                    func: func
                                )
                            )
                        }
                    );
            }

            //UIRectVertPanel<IHUDElement> GenerateGraphCompositionPanel(Func<double, int, double> modifyRes, Func<double, double, double> operation, string functionName)
            //{
            //    const double minX = -10, maxX = 10;

            //    List<Func<double, double>> functions = new()
            //    {
            //        x => (double)Algorithms.Normalize(value: MyMathHelper.Sin(x), start: -1, stop: 1),
            //        x => (double)Algorithms.Normalize(value: x, start: minX, stop: maxX),
            //        x => (double)Algorithms.Normalize(value: -x * x, start: MyMathHelper.Min(-minX * minX, -maxX * maxX), stop: 0)
            //    };

            //    var modifiedFuncs = functions.Select<Func<double, double>, Func<double, double>>
            //    (
            //        func => x => modifyRes(func(x), functions.Count)
            //    ).ToList();

            //    Func<double, double> compositionFunc = value => modifiedFuncs.Select(func => func(value)).Aggregate(operation);

            //    UIRectVertPanel<IHUDElement> graphCompositionPanel = new(childHorizPos: HorizPosEnum.Middle);
            //    graphCompositionPanel.AddChild(child: new TextBox(text: functionName));
            //    foreach (var func in modifiedFuncs)
            //        graphCompositionPanel.AddChild(child: CreateFunctionGraph(func));
            //    graphCompositionPanel.AddChild(child: new TextBox(text: "="));
            //    graphCompositionPanel.AddChild(child: CreateFunctionGraph(func: compositionFunc));
            //    return graphCompositionPanel;

            //    FunctionGraph CreateFunctionGraph(Func<double, double> func)
            //        => new
            //        (
            //            width: curUIConfig.standardUIElementWidth,
            //            height: curUIConfig.UILineHeight,
            //            backgroundColor: Color.Yellow,
            //            lineColor: Color.Red,
            //            lineWidth: 1,
            //            minX: minX,
            //            maxX: maxX,
            //            minY: 0,
            //            maxY: 1,
            //            numXSamples: 1000,
            //            func: func
            //        );
            //}
        }

        private void Initialize()
        {
            graph!.Initialize();
            lightManager.Initialize();
        }

        public void PublishMessage(IMessage message)
        {
#warning Implement this
            // If exact same message already exists, don't add it a second time
        }

        public UDouble PersonDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.PersonDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public UDouble ResDist(NodeID nodeID1, NodeID nodeID2)
            => CurGraph.ResDist(nodeID1: nodeID1, nodeID2: nodeID2);

        public MyVector2 NodePosition(NodeID nodeID)
            => CurGraph.NodePosition(nodeID: nodeID);

        public bool IsCosmicBodyActive(NodeID nodeID)
            => CurGraph.nodeIDToNode[nodeID].Active;

        public void SetIsCosmicBodyActive(NodeID nodeID, bool active)
            => CurGraph.nodeIDToNode[nodeID].Active = active;

        public void DeactivateWorldElements()
            => CurGraph.DeactivateWorldElements();

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
