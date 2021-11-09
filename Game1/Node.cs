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
using static Game1.UI.ActiveUIManager;

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

        [DataContract]
        private record ResDesinArrowEventListener([property: DataMember] Node Node, [property: DataMember] int ResInd) : IDeletedListener, INumberChangedListener
        {
            public void SyncSplittersWithArrows()
            {
                foreach (var resDestinArrow in Node.resDistribArrows[ResInd])
                    Node.resSplittersToDestins[ResInd].SetImportance
                    (
                        key: resDestinArrow.EndPos,
                        importance: (ulong)resDestinArrow.Importance
                    );
                int totalImportance = Node.resDistribArrows[ResInd].Sum(resDestinArrow => resDestinArrow.Importance);
                foreach (var resDestinArrow in Node.resDistribArrows[ResInd])
                    resDestinArrow.TotalImportance = totalImportance;
            }

            void IDeletedListener.DeletedResponse(IDeletable deletable)
            {
                if (deletable is ResDestinArrow resDestinArrow)
                {
                    Node.resDistribArrows[ResInd].RemoveChild(child: resDestinArrow);
                    Node.resSplittersToDestins[ResInd].RemoveKey(key: resDestinArrow.EndPos);
                    SyncSplittersWithArrows();
                }
                else
                    throw new ArgumentException();
            }

            void INumberChangedListener.NumberChangedResponse()
                => SyncSplittersWithArrows();
        }

        [DataContract]
        private record BuildIndustryButtonClickedListener([property: DataMember] Node Node, [property: DataMember] Construction.Params ConstrParams) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => Node.SetIndustry(newIndustry: ConstrParams.MakeIndustry(state: Node.state));
        }

        [DataContract]
        private record AddResourceDestinationButtonClickedListener : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => CurActiveUIManager.ArrowDrawingModeOn = true;
        }

        public Vector2 Position
            => state.position;
        [DataMember] public readonly float radius;
        public double LocallyProducedWatts
            => shape.Watts;

        [DataMember] private readonly NodeState state;
        [DataMember] private readonly List<Link> links;
        [DataMember] private Industry industry;
        [DataMember] private readonly UnemploymentCenter unemploymentCenter;
        [DataMember] private readonly MyArray<ProporSplitter<Vector2>> resSplittersToDestins;
        [DataMember] private ConstULongArray targetStoredResAmounts;
        [DataMember] private ULongArray undecidedResAmounts, resTravelHereAmounts;
        [DataMember] private readonly new LightCatchingDisk shape;
        [DataMember] private double remainingLocalWatts;
        [DataMember] private readonly float resDestinArrowWidth;

        [DataMember] private readonly TextBox textBox;
        [DataMember] private readonly MyArray<ToggleButton> storeToggleButtons;
        [DataMember] private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        [DataMember] private readonly UIRectPanel<IHUDElement> infoPanel, buildButtonPannel;
        [DataMember] private readonly Dictionary<Overlay, UIRectPanel<IHUDElement>> overlayTabPanels;
        [DataMember] private readonly TextBox infoTextBox;
        [DataMember] private readonly string overlayTabLabel;
        [DataMember] private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDistribArrows;
        [DataMember] private readonly ulong resDistribArrowsUILayer;

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
            undecidedResAmounts = new();
            resTravelHereAmounts = new();
            remainingLocalWatts = new();

            for (int i = 0; i < startPersonCount; i++)
            {
                Person person = Person.GeneratePerson(nodePos: Position);
                state.waitingPeople.Add(person);
            }

            textBox = new()
            {
                Text = "",
                TextColor = Color.White
            };
            textBox.Shape.Center = Position;
            AddChild(child: textBox);

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                color: Color.White,
                inactiveTabLabelColor: Color.Gray
            );

            infoPanel = new UIRectVertPanel<IHUDElement>
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

            buildButtonPannel = new UIRectVertPanel<IHUDElement>
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
            {
                Button buildIndustryButton = new
                (
                    shape: new Ellipse
                    (
                        width: 200,
                        height: 20
                    )
                    {
                        Color = Color.White
                    },
                    explanation: constrParams.explanation,
                    text: "build " + constrParams.industrParams.name
                );
                buildIndustryButton.clicked.Add
                (
                    listener: new BuildIndustryButtonClickedListener
                    (
                        Node: this,
                        ConstrParams: constrParams
                    )
                );

                buildButtonPannel.AddChild(child: buildIndustryButton);
            }

            overlayTabLabel = "overlay tab";
            overlayTabPanels = new();

            foreach (var overlay in Enum.GetValues<Overlay>())
                overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement>
                (
                    color: Color.White,
                    childHorizPos: HorizPos.Left
                );
            storeToggleButtons = new();
            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
            {
                storeToggleButtons[resInd] = new ToggleButton
                (
                    shape: new MyRectangle
                    (
                        width: 60,
                        height: 60
                    ),
                    text: "store\nswitch",
                    on: false,
                    selectedColor: Color.White,
                    deselectedColor: Color.Gray
                );

                overlayTabPanels[(Overlay)resInd].AddChild(child: storeToggleButtons[resInd]);

                Button addResourceDestinationButton = new
                (
                    shape: new MyRectangle(width: 150, height: 50)
                    {
                        Color = Color.White
                    },
                    text: $"add resource {resInd}\ndestination"
                );
                addResourceDestinationButton.clicked.Add(listener: new AddResourceDestinationButtonClickedListener());

                overlayTabPanels[(Overlay)resInd].AddChild
                (
                    child: addResourceDestinationButton
                );
            }
            UITabPanel.AddTab
            (
                tabLabelText: overlayTabLabel,
                tab: overlayTabPanels[CurWorldManager.Overlay]
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

            if (CurWorldManager.Overlay <= MaxRes)
                CurWorldManager.AddWorldUIElement
                (
                    UIElement: resDistribArrows[(int)CurWorldManager.Overlay],
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
            => storeToggleButtons[resInd].On;

        public IEnumerable<Vector2> ResDestins(int resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(int resInd)
            => targetStoredResAmounts[resInd];

        public ulong StoredResAmount(int resInd)
            => state.storedRes[resInd];

        public override void OnClick()
        {
            if (CurActiveUIManager.ArrowDrawingModeOn)
                return;
            base.OnClick();
        }

        public bool CanHaveDestin(Vector2 destination)
        {
            if (!Active || !CurActiveUIManager.ArrowDrawingModeOn)
                throw new InvalidOperationException();

            return !resSplittersToDestins[(int)CurWorldManager.Overlay].ContainsKey(destination);
        }

        public void AddResDestin(Vector2 destination)
        {
            if (!CanHaveDestin(destination: destination))
                throw new ArgumentException();

            int resInd = (int)CurWorldManager.Overlay;
            if (resInd is < 0 or > (int)MaxRes)
                throw new ArgumentOutOfRangeException();
            if (resSplittersToDestins[resInd].ContainsKey(key: destination))
                throw new ArgumentException();

            ResDestinArrow resDestinArrow = new
            (
                shape: new Arrow(startPos: Position, endPos: destination, width: resDestinArrowWidth),
                defaultActiveColor: Color.Lerp(Color.Yellow, Color.White, .5f),
                defaultInactiveColor: Color.White * .5f,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top,
                minImportance: 1,
                importance: 1,
                resInd: resInd
            );
            ResDesinArrowEventListener resDesinArrowEventListener = new(Node: this, ResInd: resInd);
            resDestinArrow.ImportanceNumberChanged.Add(listener: resDesinArrowEventListener);
            resDestinArrow.Deleted.Add(listener: resDesinArrowEventListener);

            resDistribArrows[resInd].AddChild(child: resDestinArrow);
            resDesinArrowEventListener.SyncSplittersWithArrows();
        }

        public override void OnMouseDownWorldNotMe()
        {
            if (CurActiveUIManager.ArrowDrawingModeOn)
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

            switch (CurWorldManager.Overlay)
            {
                case <= MaxRes:
                    int resInd = (int)CurWorldManager.Overlay;
                    if (IfStore(resInd: resInd))
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

            if (Active && CurActiveUIManager.ArrowDrawingModeOn)
                Arrow.DrawArrow
                (
                    startPos: Position,
                    endPos: CurWorldManager.MouseWorldPos,
                    width: resDestinArrowWidth,
                    color: Color.White * .25f
                );
        }
        
        public void SetRemainingLocalWatts(double remainingLocalWatts)
            => this.remainingLocalWatts = remainingLocalWatts;

        public override void ChoiceChangedResponse(Overlay prevOverlay)
        {
            base.ChoiceChangedResponse(prevOverlay: prevOverlay);

            UITabPanel.ReplaceTab
                (
                    tabLabelText: overlayTabLabel,
                    tab: overlayTabPanels[CurWorldManager.Overlay]
                );

            if (prevOverlay <= MaxRes)
                CurWorldManager.RemoveWorldUIElement
                (
                    UIElement: resDistribArrows[(int)prevOverlay]
                );
            if (CurWorldManager.Overlay <= MaxRes)
                CurWorldManager.AddWorldUIElement
                (
                    UIElement: resDistribArrows[(int)CurWorldManager.Overlay],
                    layer: resDistribArrowsUILayer
                );
        }

        public override void SizeOrPosChangedResponse(Shape shape)
        {
            if (shape.Center != Position)
                throw new Exception();

            base.SizeOrPosChangedResponse(shape: shape);
        }
    }
}
