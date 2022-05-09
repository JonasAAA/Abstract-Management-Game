using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class Planet : WorldUIElement, INodeAsLocalEnergyProducer, INodeAsResDestin
    {
        [Serializable]
        private readonly record struct ResDesinArrowEventListener(Planet Node, ResInd ResInd) : IDeletedListener, INumberChangedListener
        {
            public void SyncSplittersWithArrows()
            {
                foreach (var resDestinArrow in Node.resDistribArrows[ResInd])
                    Node.resSplittersToDestins[ResInd].SetImportance
                    (
                        key: resDestinArrow.DestinationId,
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
                    CurWorldManager.RemoveResDestinArrow(resInd: ResInd, resDestinArrow: resDestinArrow);
                    Node.resSplittersToDestins[ResInd].RemoveKey(key: resDestinArrow.DestinationId);
                    SyncSplittersWithArrows();
                }
                else
                    throw new ArgumentException();
            }

            void INumberChangedListener.NumberChangedResponse()
                => SyncSplittersWithArrows();
        }

        [Serializable]
        private readonly record struct BuildIndustryButtonClickedListener(Planet Node, IBuildableFactory BuildableParams) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => Node.SetIndustry(newIndustry: BuildableParams.CreateIndustry(state: Node.state));
        }

        [Serializable]
        private readonly record struct AddResourceDestinationButtonClickedListener : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => CurWorldManager.ArrowDrawingModeOn = true;
        }

        [Serializable]
        private readonly record struct ShapeParams(NodeState State) : Disk.IParams
        {
            public MyVector2 Center
                => State.position;

            public UDouble Radius
                => State.Radius;
        }

        [Serializable]
        private readonly record struct ResDestinShapeParams(NodeState State, NodeID DestinationId) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => State.position;

            public MyVector2 EndPos
                => CurWorldManager.NodePosition(nodeID: DestinationId);

            public UDouble Width
                => 2 * State.Radius;
        }

        [Serializable]
        private readonly record struct SingleFrameArrowParams(NodeState State, MyVector2 EndPos) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => State.position;

            public UDouble Width
                => 2 * State.Radius;
        }

        public NodeID NodeID
            => state.nodeID;
        public MyVector2 Position
            => state.position;
        public readonly UDouble radius;
        public UDouble LocallyProducedWatts
            => shape.Watts;

        private readonly NodeState state;
        private readonly List<Link> links;
        private Industry? industry;
        private readonly HouseOld unemploymentCenter;
        private readonly MyArray<ProporSplitter<NodeID>> resSplittersToDestins;
        private ResAmounts targetStoredResAmounts;
        private ResAmounts undecidedResAmounts, resTravelHereAmounts;
        private readonly new LightCatchingDisk shape;
        private UDouble remainingLocalWatts;

        private readonly TextBox textBox;
        private readonly MyArray<ToggleButton> storeToggleButtons;
        private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        private readonly UIRectPanel<IHUDElement> infoPanel, buildButtonPannel;
        private readonly Dictionary<IOverlay, UIRectPanel<IHUDElement>> overlayTabPanels;
        private readonly TextBox infoTextBox;
        private readonly string overlayTabLabel;
        private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDistribArrows;

        public Planet(NodeState state, Color activeColor, Color inactiveColor, int startPersonCount = 0)
            : base
            (
                shape: new LightCatchingDisk(parameters: new ShapeParams(State: state), color: Color.White),
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            shape = (LightCatchingDisk)base.shape;
            
            links = new();
            industry = null;
            unemploymentCenter = new(state: state);

            resSplittersToDestins = new
            (
                selector: resInd => new ProporSplitter<NodeID>()
            );
            targetStoredResAmounts = new();
            undecidedResAmounts = new();
            resTravelHereAmounts = new();
            remainingLocalWatts = new();

            for (int i = 0; i < startPersonCount; i++)
            {
                Person person = Person.GeneratePerson(nodeID: NodeID);
                state.waitingPeople.Add(person);
            }

            textBox = new()
            {
                Text = "",
                TextColor = Color.White
            };
            textBox.Shape.Center = Position;
            AddChild(child: textBox);

            List<(string tabLabelText, ITooltip tabTooltip, IHUDElement tab)> UITabs = new();

            infoPanel = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            UITabs.Add
            ((
                tabLabelText: "info",
                tabTooltip: new ImmutableTextTooltip(text: "Info about the planet and the industry/building on it (if such exists)"),
                tab: infoPanel
            ));
            infoTextBox = new();
            infoPanel.AddChild(child: infoTextBox);

            buildButtonPannel = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            UITabs.Add
            ((
                tabLabelText: "build",
                tabTooltip: new ImmutableTextTooltip(text: "Buildings/industries which could be built here"),
                tab: buildButtonPannel
            ));
            foreach (var buildableParams in CurIndustryConfig.constrBuildingParams)
            {
                Button buildIndustryButton = new
                (
                    shape: new Ellipse
                    (
                        width: 200,
                        height: 20,
                        color: Color.White
                    ),
                    tooltip: buildableParams.CreateTooltip(state: state),
                    text: buildableParams.ButtonName
                );
                buildIndustryButton.clicked.Add
                (
                    listener: new BuildIndustryButtonClickedListener
                    (
                        Node: this,
                        BuildableParams: buildableParams
                    )
                );

                buildButtonPannel.AddChild(child: buildIndustryButton);
            }

            overlayTabLabel = "overlay tab";
            overlayTabPanels = new();

            foreach (var overlay in IOverlay.all)
                overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement>
                (
                    color: Color.White,
                    childHorizPos: HorizPos.Left
                );
            storeToggleButtons = new();
            foreach (var resInd in ResInd.All)
            {
                storeToggleButtons[resInd] = new ToggleButton
                (
                    shape: new MyRectangle
                    (
                        width: 60,
                        height: 60,
                        color: Color.White
                    ),
                    tooltip: new ImmutableTextTooltip(text: "Specifies weather to store extra resources"),
                    text: "store\nswitch",
                    on: false,
                    selectedColor: Color.White,
                    deselectedColor: Color.Gray
                );

                overlayTabPanels[resInd].AddChild(child: storeToggleButtons[resInd]);

                Button addResourceDestinationButton = new
                (
                    shape: new MyRectangle(width: 150, height: 50, color: Color.White),
                    tooltip: new ImmutableTextTooltip(text: $"Adds new place to where {resInd} should be transported"),
                    text: $"add resource {resInd}\ndestination"
                );
                addResourceDestinationButton.clicked.Add(listener: new AddResourceDestinationButtonClickedListener());

                overlayTabPanels[resInd].AddChild
                (
                    child: addResourceDestinationButton
                );
            }
            UITabs.Add
            ((
                tabLabelText: overlayTabLabel,
                tabTooltip: new ImmutableTextTooltip(text: "UI specific to the current overlay"),
                tab: overlayTabPanels[CurWorldManager.Overlay]
            ));

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                color: Color.White,
                inactiveTabLabelColor: Color.Gray,
                tabs: UITabs
            );

            SetPopup
            (
                HUDElement: UITabPanel,
                overlays: IOverlay.all
            );

            resDistribArrows = new();
            foreach (var resInd in ResInd.All)
                resDistribArrows[resInd] = new();
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
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeID);
        }

        public void Arrive(IEnumerable<Person> people)
        {
            if (people.Count() is 0)
                return;
            state.waitingPeople.UnionWith(people);
        }

        public void Arrive(Person person)
            => state.waitingPeople.Add(person);

        public void AddResTravelHere(ResInd resInd, ulong resAmount)
            => resTravelHereAmounts = resTravelHereAmounts.WithAdd(index: resInd, value: resAmount);

        public ulong TotalQueuedRes(ResInd resInd)
            => state.storedRes[resInd] + resTravelHereAmounts[resInd];

        public bool IfStore(ResInd resInd)
            => storeToggleButtons[resInd].On;

        public IEnumerable<NodeID> ResDestins(ResInd resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(ResInd resInd)
            => targetStoredResAmounts[resInd];

        public ulong StoredResAmount(ResInd resInd)
            => state.storedRes[resInd];

        public bool CanHaveDestin(NodeID destinationId)
        {
            if (!Active || !CurWorldManager.ArrowDrawingModeOn)
                throw new InvalidOperationException();

            return destinationId != NodeID && !resSplittersToDestins[(ResInd)CurWorldManager.Overlay].ContainsKey(destinationId);
        }

        public void AddResDestin(NodeID destinationId)
        {
            if (!CanHaveDestin(destinationId: destinationId))
                throw new ArgumentException();

            ResInd resInd = (ResInd)CurWorldManager.Overlay;
            if (resSplittersToDestins[resInd].ContainsKey(key: destinationId))
                throw new ArgumentException();

            ResDestinArrow resDestinArrow = new
            (
                shapeParams: new ResDestinShapeParams
                (
                    State: state,
                    DestinationId: destinationId
                ),
                destinId: destinationId,
                defaultActiveColor: Color.Lerp(Color.Yellow, Color.White, .5f),
                defaultInactiveColor: Color.White * .5f,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top,
                minImportance: 1,
                startImportance: 1,
                resInd: resInd
            );
            ResDesinArrowEventListener resDesinArrowEventListener = new(Node: this, ResInd: resInd);
            resDestinArrow.ImportanceNumberChanged.Add(listener: resDesinArrowEventListener);
            resDestinArrow.Deleted.Add(listener: resDesinArrowEventListener);

            resDistribArrows[resInd].AddChild(child: resDestinArrow);
            CurWorldManager.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);
            resDesinArrowEventListener.SyncSplittersWithArrows();
        }

        private void SetIndustry(Industry? newIndustry)
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

        public void Update(IReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks)
        {
            // TODO: delete
            // temporary
            // state.SetRadius((double)C.Random(0.99, 1.01) * state.radius.Value);
            // temporary
            //state.position += new MyVector2(x: C.Random(min: -1.0, max: 1), y: C.Random(min: -1.0, max: 1));

            if (industry is not null)
                SetIndustry(newIndustry: industry.Update());

            // deal with people
            foreach (var person in state.waitingPeople.Clone())
            {
                NodeID? activityCenterPosition = person.ActivityCenterNodeID;
                if (activityCenterPosition is null)
                    continue;
                if (activityCenterPosition == NodeID)
                    person.Arrived();
                else
                    personFirstLinks[(NodeID, activityCenterPosition)]!.Add(start: this, person: person);
                state.waitingPeople.Remove(person);
            }

            textBox.Shape.Center = state.position;
        }

        public void UpdatePeople()
        {
            var peopleInIndustry = industry switch
            {
                null => Enumerable.Empty<Person>(),
                not null => industry.PeopleHere
            };
            foreach (var person in state.waitingPeople.Concat(unemploymentCenter.PeopleHere).Concat(peopleInIndustry))
                person.Update(lastNodeID: NodeID, closestNodeID: NodeID);
        }

        public void StartSplitRes()
        {
            targetStoredResAmounts = industry switch
            {
                null => new(),
                not null => industry.TargetStoredResAmounts()
            };

            // deal with resources
            undecidedResAmounts = state.storedRes + state.waitingResAmountsPackets.ReturnAndRemove(destination: NodeID);
            state.storedRes = new();

            state.storedRes = undecidedResAmounts.Min(resAmounts: targetStoredResAmounts);
            undecidedResAmounts -= state.storedRes;
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(Func<NodeID, INodeAsResDestin> nodeIDToNode, ResInd resInd, Func<NodeID, ulong> maxExtraResFunc)
        {
            if (undecidedResAmounts[resInd] is 0)
                return;

            var resSplitter = resSplittersToDestins[resInd];
            if (resSplitter.Empty)
                state.AddToStoredRes(resInd: resInd, resAmount: undecidedResAmounts[resInd]);
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResAmounts[resInd], maxAmountsFunc: maxExtraResFunc);
                state.AddToStoredRes(resInd: resInd, unsplitResAmount);
                foreach (var (destination, resAmount) in splitResAmounts)
                {
                    state.waitingResAmountsPackets.Add
                    (
                        destination: destination,
                        resInd: resInd,
                        resAmount: resAmount
                    );
                    nodeIDToNode(destination).AddResTravelHere(resInd: resInd, resAmount: resAmount);
                }
            }
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes(IReadOnlyDictionary<(NodeID, NodeID), Link?> resFirstLinks)
        {
            undecidedResAmounts = new();

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeID destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeID);

                resFirstLinks[(NodeID, destinationId)]!.Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            // TODO: look at this
            infoTextBox.Text = $"consists of {state.MainResAmount} {state.consistsOfResInd}\nstores {state.storedRes}\ntarget {targetStoredResAmounts}\n";

            // update text
            textBox.Text = "";

            CurWorldManager.Overlay.SwitchStatement
            (
                singleResCase: resInd =>
                {
                    if (IfStore(resInd: resInd))
                        textBox.Text += "store\n";
                    if (state.storedRes[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
                        textBox.Text += (state.storedRes[resInd] >= targetStoredResAmounts[resInd]) switch
                        {
                            true => $"have {state.storedRes[resInd] - targetStoredResAmounts[resInd]} extra resources",
                            false => $"have {(double)state.storedRes[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                        };
                },
                allResCase: () =>
                {
                    ulong totalStoredWeight = state.storedRes.TotalWeight();
                    if (totalStoredWeight > 0)
                        textBox.Text += $"stored total res weight {totalStoredWeight}";
                },
                powerCase: () => textBox.Text += $"get {shape.Watts:0.##} W from stars\nof which {shape.Watts - remainingLocalWatts:.##} W is used",
                peopleCase: () => textBox.Text += unemploymentCenter.GetInfo()
            );

            textBox.Text = textBox.Text.Trim();
            infoTextBox.Text += textBox.Text;
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();

            if (Active && CurWorldManager.ArrowDrawingModeOn)
                // TODO: could create the arrow once with endPos calculated from mouse position
                new Arrow
                (
                    parameters: new SingleFrameArrowParams
                    (
                        State: state,
                        EndPos: CurWorldManager.MouseWorldPos
                    ),
                    color: Color.White
                ).Draw();
        }
        
        public void SetRemainingLocalWatts(UDouble remainingLocalWatts)
            => this.remainingLocalWatts = remainingLocalWatts;

        public override void ChoiceChangedResponse(IOverlay prevOverlay)
        {
            base.ChoiceChangedResponse(prevOverlay: prevOverlay);

            UITabPanel.ReplaceTab
                (
                    tabLabelText: overlayTabLabel,
                    tab: overlayTabPanels[CurWorldManager.Overlay]
                );
        }
    }
}
