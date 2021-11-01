using Game1.Events;
using Game1.Industries;
using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class Node : WorldUIElement
    {
        [DataContract]
        private class UnemploymentCenter : ActivityCenter
        {
            public UnemploymentCenter(NodeState state)
                : base(activityType: ActivityType.Unemployed, energyPriority: ulong.MaxValue, state: state)
            { }

            public override bool IsFull()
                => false;

            public override double PersonScoreOfThis(Person person)
                => CurWorldConfig.personMomentumCoeff * (IsPersonHere(person: person) ? 1 : 0) 
                + (.7 * C.Random(min: 0, max: 1) + .3 * DistanceToHere(person: person)) * (1 - CurWorldConfig.personMomentumCoeff);

            public override bool IsPersonSuitable(Person person)
                // may disallow far travel
                => true;

            public override void UpdatePerson(Person person)
            {
                if (!IsPersonHere(person: person))
                    throw new ArgumentException();

                IActivityCenter.UpdatePersonDefault(person: person);
                // TODO calculate happiness
                // may decrease person's skills
            }

            public override bool CanPersonLeave(Person person)
                => true;

            public string GetText()
                => $"unemployed {peopleHere.Count}\ntravel to be unemployed\nhere {allPeople.Count - peopleHere.Count}\n";
        }

        public Vector2 Position
            => state.position;
        [DataMember]
        public readonly float radius;
        public double LocallyProducedWatts
            => shape.Watts;

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        [field:NonSerialized]
        public event Action Deleted;

        [DataMember]
        private readonly NodeState state;
        [DataMember]
        private readonly List<Link> links;
        [DataMember]
        private Industry industry;
        [DataMember]
        private readonly UnemploymentCenter unemploymentCenter;
        [DataMember]
        private readonly MyArray<ProporSplitter<Vector2>> resSplittersToDestins;
        [DataMember]
        private ConstULongArray targetStoredResAmounts;
        [DataMember]
        private readonly MyArray<bool> store;
        [DataMember]
        private ULongArray undecidedResAmounts, resTravelHereAmounts;
        [DataMember]
        private readonly new LightCatchingDisk shape;
        [DataMember]
        private double remainingLocalWatts;
        [DataMember]
        private readonly float resDestinArrowWidth;

        [NonSerialized]
        private TextBox textBox;
        [NonSerialized]
        private UIHorizTabPanel<IHUDElement<MyRectangle>> UITabPanel;
        [NonSerialized]
        private UIRectPanel<IHUDElement<NearRectangle>> infoPanel, buildButtonPannel;
        [NonSerialized]
        private Dictionary<Overlay, UIRectPanel<IHUDElement<NearRectangle>>> overlayTabPanels;
        [NonSerialized]
        private TextBox infoTextBox;
        [NonSerialized]
        private string overlayTabLabel;
        [NonSerialized]
        private MyArray<UITransparentPanel<ResDestinArrow>> resDistribArrows;
        [NonSerialized]
        private ulong resDistribArrowsUILayer;

        public Node(NodeState state, float radius, Color activeColor, Color inactiveColor, float resDestinArrowWidth, int startPersonCount = 0)
            : base(shape: new LightCatchingDisk(radius: radius), activeColor: activeColor, inactiveColor: inactiveColor, popupHorizPos: HorizPos.Right, popupVertPos: VertPos.Top)
        {
            this.state = state;
            this.resDestinArrowWidth = resDestinArrowWidth;
            shape = (LightCatchingDisk)base.shape;
            shape.Center = Position;
            
            links = new();
            industry = null;
            unemploymentCenter = new(state: state);

            resSplittersToDestins = new
            (
                values: from ind in Enumerable.Range(0, CurResConfig.ResCount)
                        select new ProporSplitter<Vector2>()
            );
            targetStoredResAmounts = new();
            store = new(value: false);
            undecidedResAmounts = new();
            resTravelHereAmounts = new();
            remainingLocalWatts = new();

            for (int i = 0; i < startPersonCount; i++)
            {
                Person person = Person.GeneratePerson(nodePos: Position);
                state.waitingPeople.Add(person);
            }
        }

        protected override void InitUninitialized()
        {
            base.InitUninitialized();

            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
                foreach (var (destination, importance) in resSplittersToDestins[resInd].Importances)
                    AddResDestin
                    (
                        resInd: resInd,
                        destination: destination,
                        importance: (int)importance
                    );

            industry?.Initialize();

            textBox = new()
            {
                Text = "",
                TextColor = Color.White
            };
            textBox.Shape.Center = Position;
            SizeOrPosChanged += () =>
            {
                if (shape.Center != Position)
                    throw new Exception();
            };
            AddChild(child: textBox);

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                color: Color.White,
                inactiveTabLabelColor: Color.Gray
            );

            infoPanel = new UIRectVertPanel<IHUDElement<NearRectangle>>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            UITabPanel.AddTab
            (
                tabLabelText: "info",
                tab: infoPanel
            );
            infoTextBox = new();
            infoPanel.AddChild(child: infoTextBox);

            buildButtonPannel = new UIRectVertPanel<IHUDElement<NearRectangle>>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            UITabPanel.AddTab
            (
                tabLabelText: "build",
                tab: buildButtonPannel
            );
            foreach (var constrParams in CurIndustryConfig.constrBuildingParams)
                buildButtonPannel.AddChild
                (
                    child: new Button<Ellipse>
                    (
                        shape: new
                        (
                            width: 200,
                            height: 20
                        )
                        {
                            Color = Color.White
                        },
                        explanation: constrParams.explanation,
                        action: () => SetIndustry(newIndustry: constrParams.MakeAndInitIndustry(state: state)),
                        text: "build " + constrParams.industrParams.name
                    )
                );

            overlayTabLabel = "overlay tab";
            overlayTabPanels = new();

            foreach (var overlay in Enum.GetValues<Overlay>())
                overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement<NearRectangle>>
                (
                    color: Color.White,
                    childHorizPos: HorizPos.Left
                );
            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
            {
                var storeToggle = new ToggleButton<MyRectangle>
                (
                    shape: new
                    (
                        width: 60,
                        height: 60
                    ),
                    text: "store\nswitch",
                    on: store[resInd],
                    selectedColor: Color.White,
                    deselectedColor: Color.Gray
                );
                // hack to make lambda expression work correctly
                int curResInd = resInd;
                storeToggle.OnChanged += () => store[curResInd] = storeToggle.On;
                overlayTabPanels[(Overlay)resInd].AddChild(child: storeToggle);

                overlayTabPanels[(Overlay)resInd].AddChild
                (
                    child: new Button<MyRectangle>
                    (
                        shape: new(width: 150, height: 50)
                        {
                            Color = Color.White
                        },
                        action: () => ActiveUIManager.ArrowDrawingModeOn = true,
                        text: $"add resource {resInd}\ndestination"
                    )
                );
            }
            UITabPanel.AddTab
            (
                tabLabelText: overlayTabLabel,
                tab: overlayTabPanels[CurOverlay]
            );

            SetPopup
            (
                UIElement: UITabPanel,
                overlays: Enum.GetValues<Overlay>()
            );

            resDistribArrowsUILayer = 1;
            resDistribArrows = new();
            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
                resDistribArrows[resInd] = new UITransparentPanel<ResDestinArrow>();

            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
                resDistribArrows[resInd].Initialize();

            if (CurOverlay <= MaxRes)
                AddWorldUIElement
                (
                    UIElement: resDistribArrows[(int)CurOverlay],
                    layer: resDistribArrowsUILayer
                );
        }

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        public void Arrive(ResAmountsPacketsByDestin resAmountsPackets)
        {
            state.waitingResAmountsPackets.Add(resAmountsPackets: resAmountsPackets);
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: Position);
        }

        public void Arrive(IEnumerable<Person> people)
        {
            if (people.Count() is 0)
                return;
            state.waitingPeople.UnionWith(people);
        }

        public void Arrive(Person person)
            => state.waitingPeople.Add(person);

        public void AddResTravelHere(int resInd, ulong resAmount)
            => resTravelHereAmounts[resInd] += resAmount;

        public ulong TotalQueuedRes(int resInd)
            => state.storedRes[resInd] + resTravelHereAmounts[resInd];

        public bool IfStore(int resInd)
            => store[resInd];

        public IEnumerable<Vector2> ResDestins(int resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(int resInd)
            => targetStoredResAmounts[resInd];

        public ulong StoredResAmount(int resInd)
            => state.storedRes[resInd];

        public override void OnClick()
        {
            if (ActiveUIManager.ArrowDrawingModeOn)
                return;
            base.OnClick();
        }

        public bool CanHaveDestin(Vector2 destination)
        {
            if (!Active || !ActiveUIManager.ArrowDrawingModeOn)
                throw new InvalidOperationException();

            return !resSplittersToDestins[(int)CurOverlay].ContainsKey(destination);
        }

        public void AddResDestin(Vector2 destination)
        {
            if (!CanHaveDestin(destination: destination))
                throw new ArgumentException();

            AddResDestin
            (
                resInd: (int)CurOverlay,
                destination: destination,
                importance: 1
            );
        }

        private void AddResDestin(int resInd, Vector2 destination, int importance)
        {
            if (resInd is < 0 or > (int)MaxRes)
                throw new ArgumentOutOfRangeException();
            if (resSplittersToDestins[resInd].ContainsKey(key: destination))
                throw new ArgumentException();
            if (importance <= 0)
                throw new ArgumentOutOfRangeException();

            ResDestinArrow resDestinArrow = new
            (
                shape: new Arrow(startPos: Position, endPos: destination, width: resDestinArrowWidth),
                defaultActiveColor: Color.Lerp(Color.Yellow, Color.White, .5f),
                defaultInactiveColor: Color.White * .5f,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top,
                minImportance: 1,
                importance: importance,
                resInd: resInd
            );
            resDestinArrow.ImportanceChanged += SyncSplittersWithArrows;
            resDestinArrow.Delete += () =>
            {
                resDistribArrows[resInd].RemoveChild(child: resDestinArrow);
                resSplittersToDestins[resInd].RemoveKey(key: destination);
                SyncSplittersWithArrows();
            };
            resDestinArrow.Initialize();

            resDistribArrows[resInd].AddChild(child: resDestinArrow);
            SyncSplittersWithArrows();

            void SyncSplittersWithArrows()
            {
                foreach (var resDestinArrow in resDistribArrows[resInd])
                    resSplittersToDestins[resInd].SetImportance
                    (
                        key: resDestinArrow.EndPos,
                        importance: (ulong)resDestinArrow.Importance
                    );
                int totalImportance = resDistribArrows[resInd].Sum(resDestinArrow => resDestinArrow.Importance);
                foreach (var resDestinArrow in resDistribArrows[resInd])
                    resDestinArrow.TotalImportance = totalImportance;
            }
        }

        public override void OnMouseDownWorldNotMe()
        {
            if (ActiveUIManager.ArrowDrawingModeOn)
                return;
            base.OnMouseDownWorldNotMe();
        }

        private void SetIndustry(Industry newIndustry)
        {
            if (industry == newIndustry)
                return;

            infoPanel.RemoveChild(child: industry?.UIElement);
            industry = newIndustry;
            if (industry is null)
                buildButtonPannel.PersonallyEnabled = true;
            else
            {
                buildButtonPannel.PersonallyEnabled = false;
                infoPanel.AddChild(child: industry.UIElement);
            }
        }

        public void Update(IReadOnlyDictionary<(Vector2, Vector2), Link> personFirstLinks)
        {
            if (industry is not null)
                SetIndustry(newIndustry: industry.Update());

            // deal with people
            foreach (var person in state.waitingPeople.Clone())
            {
                if (person.ActivityCenterPosition is null)
                    continue;

                var activityCenterPosition = person.ActivityCenterPosition.Value;
                if (activityCenterPosition == Position)
                    person.Arrived();
                else
                    personFirstLinks[(Position, activityCenterPosition)].Add(start: this, person: person);
                state.waitingPeople.Remove(person);
            }
        }

        public void UpdatePeople()
        {
            var peopleInIndustry = industry switch
            {
                null => Enumerable.Empty<Person>(),
                not null => industry.PeopleHere
            };
            foreach (var person in state.waitingPeople.Concat(unemploymentCenter.PeopleHere).Concat(peopleInIndustry))
                person.Update(prevNodePos: Position, closestNodePos: Position);
        }

        public void StartSplitRes()
        {
            targetStoredResAmounts = industry switch
            {
                null => new(),
                not null => industry.TargetStoredResAmounts()
            };

            // deal with resources
            undecidedResAmounts = state.storedRes + state.waitingResAmountsPackets.ReturnAndRemove(destination: Position);
            state.storedRes = new();

            state.storedRes = undecidedResAmounts.Min(ulongArray: targetStoredResAmounts);
            undecidedResAmounts -= state.storedRes;
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(IReadOnlyDictionary<Vector2, Node> posToNode, int resInd, Func<Vector2, ulong> maxExtraResFunc)
        {
            if (undecidedResAmounts[resInd] is 0)
                return;

            var resSplitter = resSplittersToDestins[resInd];
            if (resSplitter.Empty)
                state.storedRes[resInd] += undecidedResAmounts[resInd];
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResAmounts[resInd], maxAmountsFunc: maxExtraResFunc);
                state.storedRes[resInd] += unsplitResAmount;
                foreach (var (destination, resAmount) in splitResAmounts)
                {
                    state.waitingResAmountsPackets.Add
                    (
                        destination: destination,
                        resInd: resInd,
                        resAmount: resAmount
                    );
                    posToNode[destination].AddResTravelHere(resInd: resInd, resAmount: resAmount);
                }
            }

            undecidedResAmounts[resInd] = 0;
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes(IReadOnlyDictionary<(Vector2, Vector2), Link> resFirstLinks)
        {
            undecidedResAmounts = new();

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                Vector2 destination = resAmountsPacket.destination;
                Debug.Assert(destination != Position);

                resFirstLinks[(Position, destination)].Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            infoTextBox.Text = $"stores {state.storedRes}\ntarget {targetStoredResAmounts}";

            // update text
            textBox.Text = $"";
            if (industry is not null)
                textBox.Text += industry.GetText();

            switch (CurOverlay)
            {
                case <= MaxRes:
                    int resInd = (int)CurOverlay;
                    if (store[resInd])
                        textBox.Text += "store\n";
                    if (state.storedRes[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
                        textBox.Text += (state.storedRes[resInd] >= targetStoredResAmounts[resInd]) switch
                        {
                            true => $"have {state.storedRes[resInd] - targetStoredResAmounts[resInd]} extra resources",
                            false => $"have {(double)state.storedRes[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                        };
                    break;
                case Overlay.AllRes:
                    ulong totalStoredWeight = state.storedRes.TotalWeight();
                    if (totalStoredWeight > 0)
                        textBox.Text += $"stored total res weight {totalStoredWeight}";
                    break;
                case Overlay.Power:
                    textBox.Text += $"get {shape.Watts:0.##} W from stars\nof which {shape.Watts - remainingLocalWatts:.##} W is used";
                    break;
                case Overlay.People:
                    textBox.Text += unemploymentCenter.GetText();
                    break;
                default:
                    throw new Exception();
            };

            textBox.Text = textBox.Text.Trim();
        }

        public override void Draw()
        {
            base.Draw();

            if (Active && ActiveUIManager.ArrowDrawingModeOn)
                Arrow.DrawArrow
                (
                    startPos: Position,
                    endPos: MouseWorldPos,
                    width: resDestinArrowWidth,
                    color: Color.White * .25f
                );
        }
        
        public void SetRemainingLocalWatts(double remainingLocalWatts)
            => this.remainingLocalWatts = remainingLocalWatts;

        public override void OnOverlayChanged(Overlay oldOverlay)
        {
            base.OnOverlayChanged(oldOverlay: oldOverlay);

            UITabPanel.ReplaceTab
                (
                    tabLabelText: overlayTabLabel,
                    tab: overlayTabPanels[CurOverlay]
                );

            if (oldOverlay <= MaxRes)
                RemoveWorldUIElement
                (
                    UIElement: resDistribArrows[(int)oldOverlay]
                );
            if (CurOverlay <= MaxRes)
                AddWorldUIElement
                (
                    UIElement: resDistribArrows[(int)CurOverlay],
                    layer: resDistribArrowsUILayer
                );
        }
    }
}
