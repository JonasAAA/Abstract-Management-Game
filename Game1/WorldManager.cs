using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;

namespace Game1
{
    [Serializable]
    public class WorldManager : IDeletedListener, IClickedNowhereListener
    {
        public static WorldManager CurWorldManager { get; private set; }

        public static IEvent<IChoiceChangedListener<IOverlay>> CurOverlayChanged
            => CurWorldManager.overlayChoicePanel.choiceChanged;

        public static WorldConfig CurWorldConfig
            => CurWorldManager.worldConfig;

        public static ResConfig CurResConfig
            => CurWorldManager.resConfig;

        public static IndustryConfig CurIndustryConfig
            => CurWorldManager.industryConfig;

        private static readonly Type[] knownTypes;

        public static ActiveUIManager CreateWorldManager(GraphicsDevice graphicsDevice)
        {
            CurWorldManager = new();
            CurWorldManager.graph = CreateGraph();
            AddUIElements();
            CurWorldManager.Initialize(graphicsDevice: graphicsDevice);

            return CurWorldManager.activeUIManager;

            static void AddUIElements()
            {
                CurWorldManager.activeUIManager.AddNonHUDElement(UIElement: CurWorldManager.graph, posTransformer: CurWorldManager.worldCamera);
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
                        radius: 20,
                        center: new MyVector2(0, -300),
                        prodWatts: 200,
                        color: Color.Lerp(Color.White, Color.Red, .3f)
                    ),
                    new
                    (
                        radius: 10,
                        center: new MyVector2(200, 300),
                        prodWatts: 100,
                        color: Color.Lerp(Color.White, Color.Blue, .3f)
                    ),
                    new
                    (
                        radius: 40,
                        center: new MyVector2(-200, 100),
                        prodWatts: 400,
                        color: Color.Lerp(Color.White, new Color(0f, 1f, 0f), .3f)
                    ),
                };

                const int width = 8, height = 5, dist = 200;
                Node[,] nodes = new Node[width, height];
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        nodes[i, j] = new
                        (
                            state: new
                            (
                                position: new MyVector2(i - (width - 1) * .5, j - (height - 1) * .5) * dist,
                                radius: MyMathHelper.Pow((UDouble)2, C.Random(min: (double)3, max: 6)),
                                consistsOf: BasicResInd.Random(),
                                maxBatchDemResStored: 2
                            ),
                            activeColor: Color.White,
                            inactiveColor: Color.Gray,
                            startPersonCount: 5
                        );

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
                    nodes: from Node node in nodes
                           select node,
                    links: links
                );
            }
        }

        public static ActiveUIManager LoadWorldManager(GraphicsDevice graphicsDevice)
        {
            if (CurWorldManager is not null)
                throw new InvalidOperationException();

            CurWorldManager = Deserialize();
            CurWorldManager.Initialize(graphicsDevice: graphicsDevice);

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
                return (WorldManager)serializer.ReadObject(reader, true);
            }
        }

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
                typeof(Dictionary<IOverlay, IHUDElement>)
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

                knownTypesSet.UnionWith(ChangingValues.ExtensionMethods.GetRelatedGenericTypes(type: type));
            }
            if (unserializedTypeList.Count > 0)
                throw new Exception($"Every non-static, non-interface, non-enum type (except for Game1) must have attribute Serializable. The following types don't comply {unserializedTypeList.ToDebugString()}.");
            knownTypes = knownTypesSet.ToArray();
        }

        public IOverlay Overlay
            => overlayChoicePanel.SelectedChoiceLabel;

        public TimeSpan CurTime { get; private set; }

        public TimeSpan Elapsed { get; private set; }

        public MyVector2 MouseWorldPos
            => worldCamera.WorldPos(screenPos: (MyVector2)Mouse.GetState().Position);

        public TimeSpan MaxLinkTravelTime
            => graph.maxLinkTravelTime;

        public UDouble MaxLinkJoulesPerKg
            => graph.maxLinkJoulesPerKg;

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
                    if (graph.ActiveWorldElement is Node activeNode)
                    {
                        foreach (var node in graph.Nodes)
                            if (activeNode.CanHaveDestin(destination: node.Position))
                                node.HasDisabledAncestor = false;
                    }
                    else
                        throw new Exception();
                }
                else
                    activeUIManager.EnableAllUIElements();
            }
        }

        public readonly Dictionary<string, MyTexture> myTextures;

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

        private Graph graph;
        private bool arrowDrawingModeOn;

        private WorldManager()
        {
            worldConfig = new();
            resConfig = new();
            industryConfig = new();
            people = new();

            activityManager = new();
            energyManager = new();
            lightManager = new();

            worldCamera = new(startingWorldScale: worldConfig.startingWorldScale);
            myTextures = new();

            activeUIManager = new();
            activeUIManager.clickedNowhere.Add(listener: this);

            globalTextBox = new();
            globalTextBox.Shape.MinWidth = 300;
            globalTextBox.Shape.Color = Color.White;
            
            overlayChoicePanel = new
            (
                horizontal: true,
                choiceWidth: 100,
                choiceHeight: 30,
                selectedColor: Color.White,
                deselectedColor: Color.Gray,
                backgroundColor: Color.White
            );
            foreach (var posOverlay in IOverlay.all)
                overlayChoicePanel.AddChoice(choiceLabel: posOverlay);
            
            pauseButton = new
            (
                shape: new MyRectangle
                (
                    width: 60,
                    height: 60
                ),
                on: false,
                text: "Toggle\nPause",
                selectedColor: Color.White,
                deselectedColor: Color.Gray
            );
            
            arrowDrawingModeOn = false;
        }

        private void Initialize(GraphicsDevice graphicsDevice)
            => lightManager.Initialize(graphicsDevice: graphicsDevice);

        public void AddResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
            => graph.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);

        public void RemoveResDestinArrow(ResInd resInd, ResDestinArrow resDestinArrow)
            => graph.RemoveResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);

        public void AddHUDElement(IHUDElement HUDElement, HorizPos horizPos, VertPos vertPos)
            => activeUIManager.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);
            
        public void RemoveHUDElement(IHUDElement HUDElement)
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

            if (pauseButton.On)
                elapsed = TimeSpan.Zero;

            Elapsed = elapsed;
            CurTime += Elapsed;

            worldCamera.Update(elapsed: elapsed, canScroll: graph.MouseOn);

            lightManager.Update();

            energyManager.DistributeEnergy
            (
                nodePositions: from node in graph.Nodes
                               select node.Position,
                posToNode: graph.posToNode
            );

            graph.Update();

            activityManager.ManageActivities(people: people);

            globalTextBox.Text = (energyManager.Summary() + $"population {people.Count}").Trim();

            activeUIManager.Update(elapsed: elapsed);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            worldCamera.BeginDraw();
            graph.DrawBeforeLight();
            worldCamera.EndDraw();

            lightManager.Draw(graphicsDevice: graphicsDevice, worldToScreenTransform: worldCamera.GetToScreenTransform());

            worldCamera.BeginDraw();
            graph.DrawAfterLight();
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
