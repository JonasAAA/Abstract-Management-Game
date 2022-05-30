using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    [Serializable]
    public sealed class Planet : WorldUIElement, INodeAsLocalEnergyProducer, INodeAsResDestin, ILightCatchingObject
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
                => Node.Industry = BuildableParams.CreateIndustry(state: Node.state);
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

        private Industry? Industry
        {
            get => industry;
            set
            {
                if (industry == value)
                    return;

                infoPanel.RemoveChild(child: industry?.UIElement);
                industry = value;
                if (industry is null)
                    buildButtonPannel.PersonallyEnabled = true;
                else
                {
                    buildButtonPannel.PersonallyEnabled = false;
                    infoPanel.AddChild(child: industry.UIElement);
                }
            }
        }
        private readonly NodeState state;
        private readonly List<Link> links;
        /// <summary>
        /// NEVER use this directly, use Industry instead
        /// </summary>
        private Industry? industry;
        private readonly MyArray<ProporSplitter<NodeID>> resSplittersToDestins;
        private ResAmounts targetStoredResAmounts;
        private readonly ResPile undecidedResPile;
        private ResAmounts resTravelHereAmounts;
        private readonly new LightCatchingDisk shape;
        private UDouble usedLocalWatts;

        private readonly TextBox textBox;
        private readonly MyArray<ToggleButton> storeToggleButtons;
        private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        private readonly UIRectPanel<IHUDElement> infoPanel, buildButtonPannel;
        private readonly Dictionary<IOverlay, UIRectPanel<IHUDElement>> overlayTabPanels;
        private readonly TextBox infoTextBox;
        private readonly string overlayTabLabel;
        private readonly MyArray<UITransparentPanel<ResDestinArrow>> resDistribArrows;

        public Planet(NodeState state, Color activeColor, (House.Factory houseFactory, ulong personCount, ResPile resSource)? startingConditions = null)
            : base
            (
                shape: new LightCatchingDisk(parameters: new ShapeParams(State: state)),
                activeColor: activeColor,
                inactiveColor: state.consistsOfRes.color,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            shape = (LightCatchingDisk)base.shape;

            links = new();

            resSplittersToDestins = new
            (
                selector: resInd => new ProporSplitter<NodeID>()
            );
            targetStoredResAmounts = new();
            undecidedResPile = ResPile.CreateEmpty();
            resTravelHereAmounts = new();
            usedLocalWatts = 0;

            textBox = new(textColor: curUIConfig.almostWhiteColor);
            textBox.Shape.Center = Position;
            AddChild(child: textBox);

            List<(string tabLabelText, ITooltip tabTooltip, IHUDElement tab)> UITabs = new();

            infoPanel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
            UITabs.Add
            ((
                tabLabelText: "info",
                tabTooltip: new ImmutableTextTooltip(text: "Info about the planet and the industry/building on it (if such exists)"),
                tab: infoPanel
            ));
            infoTextBox = new();
            infoPanel.AddChild(child: infoTextBox);

            buildButtonPannel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
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
                        height: 20
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
                overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
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
                    tooltip: new ImmutableTextTooltip(text: "Specifies weather to store extra resources"),
                    text: "store\nswitch",
                    on: false
                );

                overlayTabPanels[resInd].AddChild(child: storeToggleButtons[resInd]);

                Button addResourceDestinationButton = new
                (
                    shape: new MyRectangle(width: 150, height: 50),
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

            // this is here beause it uses infoPanel, so that needs to be initialized first
            if (startingConditions is var (houseFactory, personCount, resSource))
            {
                ResAmounts houseBuildingCost = houseFactory.BuildingCost(state: state);

                {
                    var reservedBuildingRes = ReservedResPile.Create(source: resSource, resAmounts: houseBuildingCost);
                    Debug.Assert(reservedBuildingRes is not null);
                    Building? building = new
                    (
                        resSource: ref reservedBuildingRes
                    );
                    Industry = (houseFactory as IFactoryForIndustryWithBuilding).CreateIndustry
                    (
                        state: state,
                        building: ref building
                    );
                }

                for (ulong i = 0; i < personCount; i++)
                {
                    Person person = Person.GeneratePersonByMagic
                    (
                        nodeID: NodeID,
                        resSource: ReservedResPile.Create(source: resSource, resAmounts: Person.resAmountsPerPerson)!
                    );
                    state.waitingPeople.Add(person);
                }
            }
            else
                Industry = null;

            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        public void Arrive([DisallowNull] ResAmountsPacketsByDestin? resAmountsPackets)
        {
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeID);
            state.waitingResAmountsPackets.TransferAllFrom(sourcePackets: ref resAmountsPackets);
        }

        public void Arrive(IEnumerable<Person> people)
        {
            if (people.Count() is 0)
                return;
            state.waitingPeople.UnionWith(people);
        }

        public void Arrive(Person person)
            => state.waitingPeople.Add(person);

        public void AddResTravelHere(ResAmount resAmount)
            => resTravelHereAmounts = resTravelHereAmounts.WithAdd(resAmount: resAmount);

        public ulong TotalQueuedRes(ResInd resInd)
            => state.storedResPile[resInd] + resTravelHereAmounts[resInd];

        public bool IfStore(ResInd resInd)
            => storeToggleButtons[resInd].On;

        public IEnumerable<NodeID> ResDestins(ResInd resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(ResInd resInd)
            => targetStoredResAmounts[resInd];
        
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

        public void Update(IReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks)
        {
            // TODO: delete
            // temporary
            // state.SetRadius((double)C.Random(0.99, 1.01) * state.radius.Value);
            // temporary
            //state.position += new MyVector2(x: C.Random(min: -1.0, max: 1), y: C.Random(min: -1.0, max: 1));

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

            Industry = Industry?.Update();

            textBox.Shape.Center = state.position;
        }

        public void UpdatePeople()
        {
            var peopleInIndustry = Industry switch
            {
                null => Enumerable.Empty<Person>(),
                not null => Industry.PeopleHere
            };
            foreach (var person in state.waitingPeople.Concat(peopleInIndustry))
                person.Update(lastNodeID: NodeID, closestNodeID: NodeID);
        }

        public void StartSplitRes()
        {
            Debug.Assert(undecidedResPile.IsEmpty);

            targetStoredResAmounts = Industry switch
            {
                null => new(),
                not null => Industry.TargetStoredResAmounts()
            };

            // deal with resources
            state.storedResPile.TransferAllTo(destin: undecidedResPile);
            state.waitingResAmountsPackets.ReturnAndRemove(destination: NodeID).TransferAllTo(destin: undecidedResPile);

            undecidedResPile.TransferUpTo(destin: state.storedResPile, resAmounts: targetStoredResAmounts);
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(Func<NodeID, INodeAsResDestin> nodeIDToNode, ResInd resInd, Func<NodeID, ulong> maxExtraResFunc)
        {
            if (undecidedResPile[resInd] is 0)
                return;

            var resSplitter = resSplittersToDestins[resInd];
            if (resSplitter.Empty)
                undecidedResPile.TransferAllSingleResTo(destin: state.storedResPile, resInd: resInd);
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResPile[resInd], maxAmountsFunc: maxExtraResFunc);

                {
                    var unsplitResPile = ReservedResPile.Create(source: undecidedResPile, resAmount: new(resInd: resInd, amount: unsplitResAmount));
                    Debug.Assert(unsplitResPile is not null);
                    ReservedResPile.TransferAll
                    (
                        reservedSource: ref unsplitResPile,
                        destin: state.storedResPile
                    );
                }

                foreach (var (destination, resAmountNum) in splitResAmounts)
                {
                    ResAmount resAmount = new(resInd: resInd, amount: resAmountNum);
                    var resPileForDestin = ReservedResPile.Create(source: undecidedResPile, resAmount: resAmount);
                    Debug.Assert(resPileForDestin is not null);
                    state.waitingResAmountsPackets.TransferAllFrom
                    (
                        source: ref resPileForDestin,
                        destination: destination
                    );
                    nodeIDToNode(destination).AddResTravelHere(resAmount: resAmount);
                }
            }
            Debug.Assert(undecidedResPile[resInd] == 0);
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes(IReadOnlyDictionary<(NodeID, NodeID), Link?> resFirstLinks)
        {
            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeID destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeID);

                var resAmountsPacketCopy = resAmountsPacket;
                resFirstLinks[(NodeID, destinationId)]!.TransferAll(start: this, resAmountsPacket: ref resAmountsPacketCopy);
            }

            // TODO: look at this
            infoTextBox.Text = $"consists of {state.MainResAmount} {state.consistsOfResInd}\nstores {state.storedResPile}\ntarget {targetStoredResAmounts}\n";

            // update text
            textBox.Text = CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd =>
                {
                    string text = "";
                    if (IfStore(resInd: resInd))
                        text += "store\n";
                    if (state.storedResPile[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
                        text += (state.storedResPile[resInd] >= targetStoredResAmounts[resInd]) switch
                        {
                            true => $"have {state.storedResPile[resInd] - targetStoredResAmounts[resInd]} extra resources",
                            false => $"have {(double)state.storedResPile[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                        };
                    return text;
                },
                allResCase: () =>
                {
                    ulong totalStoredMass = state.storedResPile.TotalMass;
                    return totalStoredMass switch
                    {
                        > 0 => $"stored total res mass {totalStoredMass}",
                        _ => ""
                    };
                },
                powerCase: () => $"produce {(this as INodeAsLocalEnergyProducer).LocallyProducedWatts:0.##} W for local use\nof which {usedLocalWatts:0.##} W is used",
                peopleCase: () => ""
            ).Trim();

            infoTextBox.Text += textBox.Text;
        }

        protected override void DrawPreBackground(Color otherColor, Propor otherColorPropor)
        {
            base.DrawPreBackground(otherColor, otherColorPropor);

            Industry?.DrawBeforePlanet(otherColor: otherColor, otherColorPropor: otherColorPropor);
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();

            // temporary
            Industry?.DrawAfterPlanet();

            if (Active && CurWorldManager.ArrowDrawingModeOn)
                // TODO: could create the arrow once with endPos calculated from mouse position
                new Arrow
                (
                    parameters: new SingleFrameArrowParams
                    (
                        State: state,
                        EndPos: CurWorldManager.MouseWorldPos
                    )
                ).Draw(color: Color.White);
        }

        public override void ChoiceChangedResponse(IOverlay prevOverlay)
        {
            base.ChoiceChangedResponse(prevOverlay: prevOverlay);

            UITabPanel.ReplaceTab
                (
                    tabLabelText: overlayTabLabel,
                    tab: overlayTabPanels[CurWorldManager.Overlay]
                );
        }

        UDouble INodeAsLocalEnergyProducer.LocallyProducedWatts
            => Industry?.PeopleWorkOnTop switch
            {
                true or null => state.wattsHittingSurfaceOrIndustry * (UDouble).001,
                false => 0
            };

        void INodeAsLocalEnergyProducer.SetUsedLocalWatts(UDouble usedLocalWatts)
            => this.usedLocalWatts = usedLocalWatts;

        private ILightBlockingObject CurLightCatchingObject
            => Industry?.LightBlockingObject ?? shape;

        IEnumerable<double> ILightBlockingObject.RelAngles(MyVector2 lightPos)
            => CurLightCatchingObject.RelAngles(lightPos: lightPos);

        IEnumerable<double> ILightBlockingObject.InterPoints(MyVector2 lightPos, MyVector2 lightDir)
            => CurLightCatchingObject.InterPoints(lightPos: lightPos, lightDir: lightDir);

        void ILightCatchingObject.BeginSetWatts()
            => state.wattsHittingSurfaceOrIndustry = 0;

        void ILightCatchingObject.SetWatts(StarID starPos, UDouble watts, Propor powerPropor)
            => state.wattsHittingSurfaceOrIndustry += watts;
    }
}
