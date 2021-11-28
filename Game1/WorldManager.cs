using Game1.Events;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;

namespace Game1
{
    [DataContract]
    public class WorldManager : IDeletedListener, IClickedNowhereListener
    {
        public const Overlay MaxRes = (Overlay)2;

        public static WorldManager CurWorldManager { get; private set; }

        public static IEvent<IChoiceChangedListener<Overlay>> CurOverlayChanged
            => CurWorldManager.overlayChoicePanel.choiceChanged;

        public static WorldConfig CurWorldConfig
            => CurWorldManager.worldConfig;

        public static ResConfig CurResConfig
            => CurWorldManager.resConfig;

        public static IndustryConfig CurIndustryConfig
            => CurWorldManager.industryConfig;

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
                        center: new Vector2(0, -300),
                        prodWatts: 200,
                        color: Color.Lerp(Color.White, Color.Red, .3f)
                    ),
                    new
                    (
                        radius: 10,
                        center: new Vector2(200, 300),
                        prodWatts: 100,
                        color: Color.Lerp(Color.White, Color.Blue, .3f)
                    ),
                    new
                    (
                        radius: 40,
                        center: new Vector2(-200, 100),
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
                                position: new Vector2(i - (width - 1) * .5f, j - (height - 1) * .5f) * dist,
                                maxBatchDemResStored: 2
                            ),
                            radius: 32,
                            activeColor: Color.White,
                            inactiveColor: Color.Gray,
                            resDestinArrowWidth: 64,
                            startPersonCount: 5
                        );

                const double distScale = .1;

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
                                wattsPerKg: (j + 1.5) * distScale,
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
                                wattsPerKg: (j + 1) * distScale,
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
                    KnownTypes = GetKnownTypes(),
                    PreserveObjectReferences = true,
                    SerializeReadOnlyTypes = true
                }
            );

        public Overlay Overlay
            => overlayChoicePanel.SelectedChoiceLabel;

        [DataMember] public TimeSpan CurTime { get; private set; }

        [DataMember] public TimeSpan Elapsed { get; private set; }

        public Vector2 MouseWorldPos
            => worldCamera.WorldPos(screenPos: Mouse.GetState().Position.ToVector2());

        public TimeSpan MaxLinkTravelTime
            => graph.maxLinkTravelTime;

        public double MaxLinkJoulesPerKg
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
                    if (CurWorldManager.Overlay > MaxRes)
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

        [DataMember] private readonly WorldConfig worldConfig;
        [DataMember] private readonly ResConfig resConfig;
        [DataMember] private readonly IndustryConfig industryConfig;
        [DataMember] private readonly Dictionary<Person> people;
        [DataMember] private readonly EnergyManager energyManager;
        [DataMember] private readonly ActivityManager activityManager;
        [DataMember] private readonly LightManager lightManager;

        [DataMember] private readonly ActiveUIManager activeUIManager;
        [DataMember] private readonly TextBox globalTextBox;
        [DataMember] private readonly ToggleButton pauseButton;
        [DataMember] private readonly MultipleChoicePanel<Overlay> overlayChoicePanel;

        [DataMember] private Graph graph;
        [DataMember] private readonly WorldCamera worldCamera;
        [DataMember] private bool arrowDrawingModeOn;

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
            foreach (var posOverlay in Enum.GetValues<Overlay>())
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

        public void AddResDestinArrow(int resInd, ResDestinArrow resDestinArrow)
            => graph.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);

        public void RemoveResDestinArrow(int resInd, ResDestinArrow resDestinArrow)
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

        private static Type[] GetKnownTypes()
            => Assembly.GetExecutingAssembly().GetTypes().Where
            (
                type => Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) is null
                    && (Attribute.GetCustomAttribute(type, typeof(DataContractAttribute)) is not null
                    || Attribute.GetCustomAttribute(type, typeof(SerializableAttribute)) is not null)
            ).Concat
            (
                new Type[]
                {
                    typeof(UIHorizTabPanel<IHUDElement>),
                    typeof(UIHorizTabPanel<IHUDElement>.TabEnabledChangedListener),
                    typeof(MultipleChoicePanel<string>),
                    typeof(MultipleChoicePanel<string>.ChoiceEventListener),
                    typeof(MultipleChoicePanel<Overlay>),
                    typeof(MultipleChoicePanel<Overlay>.ChoiceEventListener),
                    typeof(UIRectHorizPanel<IHUDElement>),
                    typeof(UIRectHorizPanel<SelectButton>),
                    typeof(UIRectVertPanel<IHUDElement>),
                    typeof(UITransparentPanel<ResDestinArrow>),
                    typeof(Dictionary<Overlay, IHUDElement>)
                }
            ).ToArray();

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
