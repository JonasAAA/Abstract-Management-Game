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
        // TODO: rename to ResDestinArrowDeletedListener
        // TODO: delete commented out part
        [Serializable]
        private readonly record struct ResDesinArrowEventListener(Planet Node, ResInd ResInd) : IDeletedListener //, INumberChangedListener
        {
            // TODO: delete
            //public void SyncSplittersWithArrows()
            //{
            //    foreach (var resDestinArrow in Node.resDistribArrows[ResInd])
            //        Node.resSplittersToDestins[ResInd].SetImportance
            //        (
            //            key: resDestinArrow.destinationId,
            //            importance: resDestinArrow.Importance
            //        );
            //    int totalImportance = Node.resDistribArrows[ResInd].Sum(resDestinArrow => resDestinArrow.Importance);
            //    foreach (var resDestinArrow in Node.resDistribArrows[ResInd])
            //        resDestinArrow.TotalImportance = totalImportance;
            //}

            void IDeletedListener.DeletedResponse(IDeletable deletable)
            {
                if (deletable is ResDestinArrow resDestinArrow)
                {
                    
                    Node.resDistribArrows[ResInd].RemoveChild(child: resDestinArrow);
                    CurWorldManager.RemoveResDestinArrow(resInd: ResInd, resDestinArrow: resDestinArrow);
                    Node.resSplittersToDestins[ResInd].RemoveKey(key: resDestinArrow.destinationId);
                    // TODO: delete
                    //resDestinArrow.Importance = 0;
                    //SyncSplittersWithArrows();
                }
                else
                    throw new ArgumentException();
            }

            // TODO: delete
            //void INumberChangedListener.NumberChangedResponse()
            //    => SyncSplittersWithArrows();
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
        private record ShapeParams : LateInitializer<Planet>, Disk.IParams
        {
            public MyVector2 Center
                => Param.Position;

            public UDouble Radius
                => Param.state.Radius;

            public bool Active
                => Param.Active;
        }

        [Serializable]
        private record ResDestinArrowParams : ResDestinArrow.IParamsAndState
        {
            public UDouble Width
                => 2 * State.Radius;

            public ulong TotalImportance
                => planet.totalResDesinArrowImportance;

            public Color IncrAndDecrButtonColor
                => Color.Blue;

            public Color BackgroundColor
                => Color.White;

            public ulong MinImportance
                => 1;

            public ulong Importance
            {
                get => planet.resSplittersToDestins[resInd].GetImportance(key: destinationId);
                set
                {
                    planet.totalResDesinArrowImportance -= Importance;
                    planet.resSplittersToDestins[resInd].SetImportance
                    (
                        key: destinationId,
                        importance: value
                    );
                    planet.totalResDesinArrowImportance += Importance;
                }
            }

            public Color DefaultActiveColor
                => Color.Lerp(Color.Yellow, Color.White, .5f);

            public Color DefaultInactiveColor
                => Color.White * .5f;

            private NodeState State
                => planet.state;

            private readonly Planet planet;
            private readonly NodeId destinationId;
            private readonly ResInd resInd;

            public ResDestinArrowParams(Planet planet, NodeId destinationId, ResInd resInd)
            {
                this.planet = planet;
                this.destinationId = destinationId;
                this.resInd = resInd;
                Importance = MinImportance;
            }
        }

        [Serializable]
        private readonly record struct SingleFrameArrowParams(NodeState State, MyVector2 EndPos) : Arrow.IParams
        {
            public MyVector2 StartPos
                => State.position;

            public UDouble Width
                => 2 * State.Radius;

            public bool Active
                => true;

            Color WorldShape.IParams.ActiveColor
                => Color.White * .25f;
        }

        [Serializable]
        private record OnPlanetTextBoxParams(Planet Planet) : TextBox.IParams
        {
            public string? Text
                => CurWorldManager.Overlay.SwitchExpression
                (
                    singleResCase: resInd =>
                    {
                        string text = "";
                        if (Planet.IfStore(resInd: resInd))
                            text += "store\n";
                        if (State.storedRes[resInd] is not 0 || Planet.targetStoredResAmounts[resInd] is not 0)
                            text += (State.storedRes[resInd] >= Planet.targetStoredResAmounts[resInd]) switch
                            {
                                true => $"have {State.storedRes[resInd] - Planet.targetStoredResAmounts[resInd]} extra resources",
                                false => $"have {(double)State.storedRes[resInd] / Planet.targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                            };
                        return text;
                    },
                    allResCase: () => $"stored total res weight {State.storedRes.TotalWeight()}",
                    powerCase: () => $"get {Planet.shape.Watts:0.##} W from stars\nof which {Planet.shape.Watts - Planet.remainingLocalWatts:.##} W is used",
                    peopleCase: () => Planet.unemploymentCenter.GetInfo()
                ).Trim();

            public Color BackgroundColor
                => Color.Transparent;

            private NodeState State
                => Planet.state;
        }

        [Serializable]
        private record InfoTextBoxParams(Planet Planet, OnPlanetTextBoxParams OnPlanetTextBoxParams) : TextBox.IParams
        {
            public string Text
                => $"consists of {State.MainResAmount} {State.consistsOfResInd}\nstores {State.storedRes}\ntarget {Planet.targetStoredResAmounts}\n" + OnPlanetTextBoxParams.Text;

            public Color BackgroundColor
                => Color.Transparent;

            private NodeState State
                => Planet.state;
        }

        [Serializable]
        private record BuildIndustryButtonParams(IBuildableFactory BuildableFactory) : Button.IParams
        {
            public string? Text
                => BuildableFactory.ButtonName;

            public string? Explanation
                => BuildableFactory.Explanation;
        }

        [Serializable]
        private record AddResDestinButtonParams(ResInd ResInd) : Button.IParams
        {
            public string? Text
                => $"add resource {ResInd}\ndestination";

            public string? Explanation
                => $"This is basically equivalent to placing a recurring order to get {ResInd} to a new destination";
        }

        [Serializable]
        private record StoreToggleButtonParams(ResInd ResInd) : ToggleButton.IParams
        {
            public string? Explanation
                => null;

            public string? Text
                => $"store {ResInd}\nswitch";

            Color OnOffButton.IParams.SelectedColor
                => Color.White;

            Color OnOffButton.IParams.DeselectedColor
                => Color.Gray;
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
        private Industry? industry;
        private readonly HouseOld unemploymentCenter;
        private readonly MyArray<ProporSplitter<NodeId>> resSplittersToDestins;
        private ResAmounts targetStoredResAmounts;
        private ResAmounts undecidedResAmounts, resTravelHereAmounts;
        private readonly new LightCatchingDisk shape;
        private UDouble remainingLocalWatts;
        private ulong totalResDesinArrowImportance;

        private readonly TextBox onPlanetTextBox;
        private readonly MyArray<ToggleButton> storeToggleButtons;
        private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        private readonly UIRectPanel<IHUDElement> infoPanel, buildButtonPannel;
        private readonly Dictionary<IOverlay, UIRectPanel<IHUDElement>> overlayTabPanels;
        private readonly TextBox infoTextBox;
        private readonly string overlayTabLabel;
        private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDistribArrows;

        public Planet(NodeState state, int startPersonCount = 0)
            : base
            (
                shape: new LightCatchingDisk(parameters: new ShapeParams()),
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            ShapeParams.InitializeLast(param: this);
            this.state = state;
            shape = (LightCatchingDisk)base.shape;
            
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

            OnPlanetTextBoxParams onPlanetTextBoxParams = new(Planet: this);
            onPlanetTextBox = new(parameters: onPlanetTextBoxParams);
            onPlanetTextBox.Shape.Center = Position;
            AddChild(child: onPlanetTextBox);

            List<(string tabLabelText, IHUDElement tab)> UITabs = new();

            infoPanel = new UIRectVertPanel<IHUDElement>
            (
                parameters: new UIRectVertPanel<IHUDElement>.ImmutableParams(backgroundColor: Color.White),
                childHorizPos: HorizPos.Left
            );
            UITabs.Add
            ((
                tabLabelText: "info",
                tab: infoPanel
            ));
            infoTextBox = new
            (
                parameters: new InfoTextBoxParams
                (
                    Planet: this,
                    OnPlanetTextBoxParams: onPlanetTextBoxParams
                )
            );
            infoPanel.AddChild(child: infoTextBox);

            buildButtonPannel = new UIRectVertPanel<IHUDElement>
            (
                parameters: new UIRectVertPanel<IHUDElement>.ImmutableParams(backgroundColor: Color.White),
                childHorizPos: HorizPos.Left
            );
            UITabs.Add
            ((
                tabLabelText: "build",
                tab: buildButtonPannel
            ));
            foreach (var buildableFactory in CurIndustryConfig.buildableFactories)
            {
                Button buildIndustryButton = new
                (
                    shape: new Ellipse
                    (
                        width: 200,
                        height: 20,
                        parameters: new Ellipse.ImmutableParams(color: Color.White)
                    ),
                    parameters: new BuildIndustryButtonParams(BuildableFactory: buildableFactory)
                );
                buildIndustryButton.clicked.Add
                (
                    listener: new BuildIndustryButtonClickedListener
                    (
                        Node: this,
                        BuildableParams: buildableFactory
                    )
                );

                buildButtonPannel.AddChild(child: buildIndustryButton);
            }

            overlayTabLabel = "overlay tab";
            overlayTabPanels = new();

            foreach (var overlay in IOverlay.all)
                overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement>
                (
                    new UIRectVertPanel<IHUDElement>.ImmutableParams(backgroundColor: Color.White),
                    childHorizPos: HorizPos.Left
                );
            storeToggleButtons = new();
            foreach (var resInd in ResInd.All)
            {
                storeToggleButtons[resInd] = new ToggleButton
                (
                    shapeFactory: new MyRectangle.Factory(),
                    width: 60,
                    height: 60,
                    parameters: new StoreToggleButtonParams(ResInd: resInd),
                    on: false
                );

                overlayTabPanels[resInd].AddChild(child: storeToggleButtons[resInd]);

                Button addResourceDestinationButton = new
                (
                    shape: new MyRectangle
                    (
                        width: 150,
                        height: 50,
                        parameters: new MyRectangle.ImmutableParams(color: Color.White)
                    ),
                    parameters: new AddResDestinButtonParams(ResInd: resInd)
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
                paramsAndState: new ResDestinArrowParams
                (
                    planet: this,
                    destinationId: destinationId,
                    resInd: resInd
                ),
                sourceId: NodeId,
                destinationId: destinationId,
                resInd: resInd,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            );
            // TODO: delete
            //ResDesinArrowEventListener resDesinArrowEventListener = new(Node: this, ResInd: resInd);
            //resDestinArrow.ImportanceNumberChanged.Add(listener: resDesinArrowEventListener);
            //resDestinArrow.Deleted.Add(listener: resDesinArrowEventListener);
            resDestinArrow.Deleted.Add(listener: new ResDesinArrowEventListener(Node: this, ResInd: resInd));

            resDistribArrows[resInd].AddChild(child: resDestinArrow);
            CurWorldManager.AddResDestinArrow(resInd: resInd, resDestinArrow: resDestinArrow);
            // TODO: delete
            //resDesinArrowEventListener.SyncSplittersWithArrows();
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

        public void Update(IReadOnlyDictionary<(NodeId, NodeId), Link?> personFirstLinks)
        {
            // TODO: delete
            // temporary
            // state.SetRadius((double)C.Random(0.99, 1.01) * state.radius.Value);
            // temporary
            // state.position += new MyVector2(x: C.Random(min: -1.0, max: 1), y: C.Random(min: -1.0, max: 1));

            if (industry is not null)
                SetIndustry(newIndustry: industry.Update());

            // deal with people
            foreach (var person in state.waitingPeople.Clone())
            {
                NodeId? activityCenterPosition = person.ActivityCenterNodeId;
                if (activityCenterPosition is null)
                    continue;
                if (activityCenterPosition == NodeId)
                    person.Arrived();
                else
                    personFirstLinks[(NodeId, activityCenterPosition)]!.Add(start: this, person: person);
                state.waitingPeople.Remove(person);
            }

            onPlanetTextBox.Shape.Center = Position;
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
        public void EndSplitRes(IReadOnlyDictionary<(NodeId, NodeId), Link?> resFirstLinks)
        {
            undecidedResAmounts = new();

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeId destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeId);

                resFirstLinks[(NodeId, destinationId)]!.Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            // TODO: delete
            //// TODO: look at this
            //infoTextBox.Text = $"consists of {state.MainResAmount} {state.consistsOfResInd}\nstores {state.storedRes}\ntarget {targetStoredResAmounts}\n";

            ////update text
            //onPlanetTextBox.Text = "";

            //CurWorldManager.Overlay.SwitchStatement
            //(
            //    singleResCase: resInd =>
            //    {
            //        if (IfStore(resInd: resInd))
            //            onPlanetTextBox.Text += "store\n";
            //        if (state.storedRes[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
            //            onPlanetTextBox.Text += (state.storedRes[resInd] >= targetStoredResAmounts[resInd]) switch
            //            {
            //                true => $"have {state.storedRes[resInd] - targetStoredResAmounts[resInd]} extra resources",
            //                false => $"have {(double)state.storedRes[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
            //            };
            //    },
            //    allResCase: () =>
            //    {
            //        ulong totalStoredWeight = state.storedRes.TotalWeight();
            //        if (totalStoredWeight > 0)
            //            onPlanetTextBox.Text += $"stored total res weight {totalStoredWeight}";
            //    },
            //    powerCase: () => onPlanetTextBox.Text += $"get {shape.Watts:0.##} W from stars\nof which {shape.Watts - remainingLocalWatts:.##} W is used",
            //    peopleCase: () => onPlanetTextBox.Text += unemploymentCenter.GetInfo()
            //);

            //onPlanetTextBox.Text = onPlanetTextBox.Text.Trim();
            //infoTextBox.Text += onPlanetTextBox.Text;
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
                        EndPos: CurWorldManager.MouseWorldPos
                    )
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
