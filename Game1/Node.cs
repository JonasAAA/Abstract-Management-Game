using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    // TODO: could rename to Planet
    [Serializable]
    public class Node : WorldUIElement, INodeAsLocalEnergyProducer, INodeAsResDestin
    {
        [Serializable]
        private readonly record struct ResDesinArrowEventListener(Node Node, ResInd ResInd) : IDeletedListener, INumberChangedListener
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
        private readonly record struct BuildIndustryButtonClickedListener(Node Node, IBuildableFactory BuildableParams) : IClickedListener
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
            public UDouble radius
                => State.radius;
        }

        [Serializable]
        private readonly record struct ResDestinArrowParams(NodeState State, NodeId DestinationId, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int importance, ResInd resInd) : ResDestinArrow.IParams
        {
            public MyVector2 startPos
                => State.position;

            public MyVector2 endPos
                => CurWorldManager.NodePosition(nodeId: DestinationId);

            public UDouble width
                => 2 * State.radius;

            public NodeId SourceId
                => State.nodeId;
        }

        [Serializable]
        private readonly record struct SingleFrameArrowParams(NodeState State, MyVector2 endPos) : VectorShape.IParams
        {
            public MyVector2 startPos
                => State.position;

            public UDouble width
                => 2 * State.radius;
        }

        public NodeId NodeId
            => state.nodeId;
        public MyVector2 Position
            => state.position;
        public readonly UDouble radius;
        public UDouble LocallyProducedWatts
            => shape.Watts;

        private readonly NodeState state;
        private readonly List<Link> links;
        private Industry industry;
        private readonly HouseOld unemploymentCenter;
        private readonly MyArray<ProporSplitter<NodeId>> resSplittersToDestins;
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

        public Node(NodeState state, Color activeColor, Color inactiveColor, int startPersonCount = 0)
            : base
            (
                shape: new LightCatchingDisk(parameters: new ShapeParams(State: state)),
                activeColor: activeColor,
                inactiveColor: inactiveColor,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            shape = (LightCatchingDisk)base.shape;
            shape.Center = Position;
            
            links = new();
            industry = null;
            unemploymentCenter = new(state: state);

            resSplittersToDestins = new
            (
                selector: resInd => new ProporSplitter<NodeId>()
            );
            targetStoredResAmounts = new();
            undecidedResAmounts = new();
            resTravelHereAmounts = new();
            remainingLocalWatts = new();

            for (int i = 0; i < startPersonCount; i++)
            {
                Person person = Person.GeneratePerson(nodeId: NodeId);
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
            foreach (var buildableParams in CurIndustryConfig.constrBuildingParams)
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
                    explanation: buildableParams.Explanation,
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
                        height: 60
                    ),
                    text: "store\nswitch",
                    on: false,
                    selectedColor: Color.White,
                    deselectedColor: Color.Gray
                );

                overlayTabPanels[resInd].AddChild(child: storeToggleButtons[resInd]);

                Button addResourceDestinationButton = new
                (
                    shape: new MyRectangle(width: 150, height: 50)
                    {
                        Color = Color.White
                    },
                    text: $"add resource {resInd}\ndestination"
                );
                addResourceDestinationButton.clicked.Add(listener: new AddResourceDestinationButtonClickedListener());

                overlayTabPanels[resInd].AddChild
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
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeId);
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

        public IEnumerable<NodeId> ResDestins(ResInd resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(ResInd resInd)
            => targetStoredResAmounts[resInd];

        public ulong StoredResAmount(ResInd resInd)
            => state.storedRes[resInd];

        public bool CanHaveDestin(NodeId destinationId)
        {
            if (!Active || !CurWorldManager.ArrowDrawingModeOn)
                throw new InvalidOperationException();

            return destinationId != NodeId && !resSplittersToDestins[(ResInd)CurWorldManager.Overlay].ContainsKey(destinationId);
        }

        public void AddResDestin(NodeId destinationId)
        {
            if (!CanHaveDestin(destinationId: destinationId))
                throw new ArgumentException();

            ResInd resInd = (ResInd)CurWorldManager.Overlay;
            if (resSplittersToDestins[resInd].ContainsKey(key: destinationId))
                throw new ArgumentException();

            ResDestinArrow resDestinArrow = new
            (
                parameters: new ResDestinArrowParams
                (
                    State: state,
                    DestinationId: destinationId,
                    defaultActiveColor: Color.Lerp(Color.Yellow, Color.White, .5f),
                    defaultInactiveColor: Color.White * .5f,
                    popupHorizPos: HorizPos.Right,
                    popupVertPos: VertPos.Top,
                    minImportance: 1,
                    importance: 1,
                    resInd: resInd
                )
            );
            ResDesinArrowEventListener resDesinArrowEventListener = new(Node: this, ResInd: resInd);
            resDestinArrow.ImportanceNumberChanged.Add(listener: resDesinArrowEventListener);
            resDestinArrow.Deleted.Add(listener: resDesinArrowEventListener);

            resDistribArrows[resInd].AddChild(child: resDestinArrow);
            CurWorldManager.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);
            resDesinArrowEventListener.SyncSplittersWithArrows();
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

        public void Update(IReadOnlyDictionary<(NodeId, NodeId), Link> personFirstLinks)
        {
            // TODO: delete
            // temporary
            // state.SetRadius((double)C.Random(0.99, 1.01) * state.radius.Value);

            if (industry is not null)
                SetIndustry(newIndustry: industry.Update());

            // deal with people
            foreach (var person in state.waitingPeople.Clone())
                if (person.ActivityCenterNodeId is NodeId activityCenterPosition)
                {
                    if (activityCenterPosition == NodeId)
                        person.Arrived();
                    else
                        personFirstLinks[(NodeId, activityCenterPosition)].Add(start: this, person: person);
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
                person.Update(lastNodeId: NodeId, closestNodeId: NodeId);
        }

        public void StartSplitRes()
        {
            targetStoredResAmounts = industry switch
            {
                null => new(),
                not null => industry.TargetStoredResAmounts()
            };

            // deal with resources
            undecidedResAmounts = state.storedRes + state.waitingResAmountsPackets.ReturnAndRemove(destination: NodeId);
            state.storedRes = new();

            state.storedRes = undecidedResAmounts.Min(resAmounts: targetStoredResAmounts);
            undecidedResAmounts -= state.storedRes;
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(Func<NodeId, INodeAsResDestin> nodeIdToNode, ResInd resInd, Func<NodeId, ulong> maxExtraResFunc)
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
                    nodeIdToNode(destination).AddResTravelHere(resInd: resInd, resAmount: resAmount);
                }
            }
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes(IReadOnlyDictionary<(NodeId, NodeId), Link> resFirstLinks)
        {
            undecidedResAmounts = new();

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeId destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeId);

                resFirstLinks[(NodeId, destinationId)].Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            // TODO: look at this
            infoTextBox.Text = $"consists of {state.mainResAmount} {state.consistsOfResInd}\nstores {state.storedRes}\ntarget {targetStoredResAmounts}\n";

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

        public override void Draw()
        {
            base.Draw();

            if (Active && CurWorldManager.ArrowDrawingModeOn)
                // TODO: could create the arrow once with endPos calculated from mouse position
                new Arrow
                (
                    parameters: new SingleFrameArrowParams
                    (
                        State: state,
                        endPos: CurWorldManager.MouseWorldPos
                    )
                )
                {
                    Color = Color.White * .25f
                }.Draw();
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
