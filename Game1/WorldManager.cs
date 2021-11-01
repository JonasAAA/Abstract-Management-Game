using Game1.Events;
using Game1.Industries;
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
    public class WorldManager : IDeletedListener
    {
        public const Overlay MaxRes = (Overlay)2;

        public static WorldManager Current { get; private set; }

        public static Overlay CurOverlay
            => Current.overlay;

        public static Event<ICurOverlayChangedListener> CurOverlayChanged
            => Current.curOverlayChanged;

        public static WorldConfig CurWorldConfig
            => Current.worldConfig;

        public static ResConfig CurResConfig
            => Current.resConfig;

        public static IndustryConfig CurIndustryConfig
            => Current.industryConfig;
        
        public static TimeSpan CurTime { get; private set; }

        public static TimeSpan Elapsed { get; private set; }

        public static Vector2 MouseWorldPos
            => Current.worldCamera.WorldPos(screenPos: Mouse.GetState().Position.ToVector2());

        public static TimeSpan MaxLinkTravelTime
            => Current.graph.maxLinkTravelTime;

        public static double MaxLinkJoulesPerKg
            => Current.graph.maxLinkJoulesPerKg;

        public static Graph Create(GraphicsDevice graphicsDevice)
        {
            if (Current is not null)
                throw new InvalidOperationException();
            Current = new();
            Current.Initialize(graphicsDevice: graphicsDevice);
            return Current.graph;
        }

        public static Graph Load(GraphicsDevice graphicsDevice)
        {
            if (Current is not null)
                throw new InvalidOperationException();

            Current = Deserialize();
            Current.Initialize(graphicsDevice: graphicsDevice);
            return Current.graph;

            static WorldManager Deserialize()
            {
                using FileStream fileStream = new(path: @"C:\Users\Jonas\Desktop\Abstract Management Game\save.bin", FileMode.Open);
                DataContractSerializerSettings serializerSettings = new()
                {
                    KnownTypes = GetKnownTypes(),
                    PreserveObjectReferences = true,
                    SerializeReadOnlyTypes = true
                };
                DataContractSerializer serializer = new
                (
                    type: typeof(WorldManager),
                    settings: serializerSettings
                );
                XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(fileStream, new XmlDictionaryReaderQuotas());
                return (WorldManager)serializer.ReadObject(reader, true);
            }
        }

        public static void AddWorldUIElement(IUIElement UIElement, ulong layer)
            => Current.graph.AddUIElement(UIElement: UIElement, layer: layer);

        public static void RemoveWorldUIElement(IUIElement UIElement)
            => Current.graph.RemoveUIElement(UIElement: UIElement);

        public static void AddEnergyProducer(IEnergyProducer energyProducer)
            => Current.energyManager.AddEnergyProducer(energyProducer: energyProducer);

        public static void AddEnergyConsumer(IEnergyConsumer energyConsumer)
            => Current.energyManager.AddEnergyConsumer(energyConsumer: energyConsumer);

        public static void AddActivityCenter(IActivityCenter activityCenter)
            => Current.activityManager.AddActivityCenter(activityCenter: activityCenter);

        public static void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
            => Current.lightManager.AddLightCatchingObject(lightCatchingObject: lightCatchingObject);

        public static void AddLightSource(ILightSource lightSource)
            => Current.lightManager.AddLightSource(lightSource: lightSource);

        public static void AddPerson(Person person)
        {
            Current.people.Add(person);
            person.Deleted.Add(listener: Current);
        }

        [DataMember]
        private readonly Event<ICurOverlayChangedListener> curOverlayChanged;
        [DataMember]
        private readonly WorldConfig worldConfig;
        [DataMember]
        private readonly ResConfig resConfig;
        [DataMember]
        private readonly IndustryConfig industryConfig;
        [DataMember]
        private readonly MyHashSet<Person> people;
        [DataMember]
        private Graph graph;
        [DataMember]
        private WorldCamera worldCamera;
        [DataMember]
        private EnergyManager energyManager;
        [DataMember]
        private ActivityManager activityManager;
        [DataMember]
        private LightManager lightManager;
        [DataMember]
        private Overlay overlay;
        [DataMember]
        private bool paused;

        private readonly TextBox globalTextBox;

        private WorldManager()
        {
            curOverlayChanged = new();
            worldConfig = new();
            resConfig = new();
            ConstArray.Initialize(resCount: resConfig.ResCount);
            industryConfig = new();
            people = new();

            overlay = Overlay.Res0;
            paused = false;

            globalTextBox = new();
            globalTextBox.Shape.MinWidth = 300;
            globalTextBox.Shape.Color = Color.White;
        }

        private void Initialize(GraphicsDevice graphicsDevice)
        {
            worldCamera = new(graphicsDevice: graphicsDevice);
            activityManager = new();
            energyManager = new();
            lightManager = new();
            lightManager.Initialize(graphicsDevice: graphicsDevice);

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

            const int minSafeDist = 100;
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
                            minSafeDist: minSafeDist
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
                            minSafeDist: minSafeDist
                        )
                    );

            graph = new
            (
                stars: stars,
                nodes: from Node node in nodes
                       select node,
                links: links
            );
            graph.Initialize();

            ActiveUIManager.AddHUDElement
            (
                UIElement: globalTextBox,
                horizPos: HorizPos.Left,
                vertPos: VertPos.Top
            );

            ToggleButton<MyRectangle> pauseButton = new
            (
                shape: new
                (
                    width: 60,
                    height: 60
                ),
                on: false,
                text: "Toggle\nPause",
                selectedColor: Color.White,
                deselectedColor: Color.Gray
            );

            pauseButton.OnChanged += () => paused = pauseButton.On;

            ActiveUIManager.AddHUDElement
            (
                UIElement: pauseButton,
                horizPos: HorizPos.Right,
                vertPos: VertPos.Bottom
            );

            MultipleChoicePanel overlayChoicePanel = new
            (
                horizontal: true,
                choiceWidth: 100,
                choiceHeight: 30,
                selectedColor: Color.White,
                deselectedColor: Color.Gray,
                backgroundColor: Color.White
            );

            foreach (var posOverlay in Enum.GetValues<Overlay>())
                overlayChoicePanel.AddChoice
                (
                    choiceText: posOverlay.ToString(),
                    select: () =>
                    {
                        if (CurOverlay == posOverlay)
                            return;
                        Overlay oldOverlay = CurOverlay;
                        overlay = posOverlay;
                        curOverlayChanged.Raise(action: listener => listener.OnOverlayChanged(oldOverlay: oldOverlay));
                    }
                );

            ActiveUIManager.AddHUDElement
            (
                UIElement: overlayChoicePanel,
                horizPos: HorizPos.Middle,
                vertPos: VertPos.Top
            );
        }

        public void Update(TimeSpan elapsed)
        {
            if (elapsed < TimeSpan.Zero)
                throw new ArgumentException();

            if (paused)
                elapsed = TimeSpan.Zero;

            Elapsed = elapsed;
            CurTime += Elapsed;

            worldCamera.Update(elapsed: elapsed, canScroll: !ActiveUIManager.MouseAboveHUD);

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
        }

        public void Serialize()
        {
            using FileStream fileStream = new(path: @"C:\Users\Jonas\Desktop\Abstract Management Game\save.bin", FileMode.Create);
            DataContractSerializerSettings serializerSettings = new()
            {
                KnownTypes = GetKnownTypes(),
                PreserveObjectReferences = true,
                SerializeReadOnlyTypes = true
            };
            DataContractSerializer serializer = new
            (
                type: typeof(WorldManager),
                settings: serializerSettings
            );

            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(fileStream);
            serializer.WriteObject(writer, this);
        }

        private static Type[] GetKnownTypes()
            => Assembly.GetExecutingAssembly().GetTypes().Where
            (
                type => Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) is null
                    && Attribute.GetCustomAttribute(type, typeof(DataContractAttribute)) is not null
            ).ToArray();

        void IDeletedListener.Deleted(object deletable)
        {
            if (deletable is Person person)
                people.Remove(person);
            else
                throw new ArgumentException();
        }
    }
}
