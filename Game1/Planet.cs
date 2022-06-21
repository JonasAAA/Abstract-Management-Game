﻿using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using System.Diagnostics.CodeAnalysis;
using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class Planet : WorldUIElement, ILinkFacingPlanet, INodeAsLocalEnergyProducer, INodeAsResDestin, ILightCatchingObject
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
                => State.Position;

            public UDouble Radius
                => State.Radius;
        }

        [Serializable]
        private readonly record struct ResDestinShapeParams(NodeState State, NodeID DestinationId) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => State.Position;

            public MyVector2 EndPos
                => CurWorldManager.NodePosition(nodeID: DestinationId);

            public UDouble Width
                => 2 * State.Radius;
        }

        [Serializable]
        private readonly record struct SingleFrameArrowParams(NodeState State, MyVector2 EndPos) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => State.Position;

            public UDouble Width
                => 2 * State.Radius;
        }

        public NodeID NodeID
            => state.NodeID;
        public MyVector2 Position
            => state.Position;

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
        /// NEVER use this directly, use Planet.Industry instead
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
                inactiveColor: state.ConsistsOfRes.color,
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
            targetStoredResAmounts = ResAmounts.Empty;
            undecidedResPile = ResPile.CreateEmpty();
            resTravelHereAmounts = ResAmounts.Empty;
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

            ulong startingNonPlanetMass = 0;
            // this is here beause it uses infoPanel, so that needs to be initialized first
            if (startingConditions is var (houseFactory, personCount, resSource))
            {
                ResAmounts houseBuildingCost = houseFactory.BuildingCost(state: state);

                {
                    var reservedBuildingRes = ReservedResPile.CreateIfHaveEnough(source: resSource, resAmounts: houseBuildingCost);
                    Debug.Assert(reservedBuildingRes is not null);
                    startingNonPlanetMass += reservedBuildingRes.Mass;
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
                    RealPerson.GeneratePersonByMagic
                    (
                        nodeID: NodeID,
                        resSource: ReservedResPile.CreateIfHaveEnough(source: resSource, resAmounts: RealPerson.resAmountsPerPerson)!,
                        personDestin: state.WaitingPeople
                    );
                }
                startingNonPlanetMass += state.WaitingPeople.Mass;
            }
            else
                Industry = null;
            state.Initialize(startingNonPlanetMass: startingNonPlanetMass);

            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public ulong TotalQueuedRes(ResInd resInd)
            => state.StoredResPile[resInd] + resTravelHereAmounts[resInd];

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
            state.WaitingPeople.ForEach
            (
                personalAction: realPerson =>
                {
                    NodeID? activityCenterPosition = realPerson.ActivityCenterNodeID;
                    if (activityCenterPosition is null)
                        return;
                    if (activityCenterPosition == NodeID)
                        realPerson.Arrived(personSource: state.WaitingPeople);
                    else
                        personFirstLinks[(NodeID, activityCenterPosition)]!.TransferFrom(start: this, peopleSource: state.WaitingPeople, person: realPerson);
                }
            );

            Industry = Industry?.Update();

            textBox.Shape.Center = state.Position;
        }

        public void UpdatePeople()
        {
            RealPerson.UpdateParams personUpdateParams = new(LastNodeID: NodeID, ClosestNodeID: NodeID);
            Industry?.UpdatePeople(updateParams: personUpdateParams);
            state.WaitingPeople.Update(updateParams: personUpdateParams, personalUpdate: null);
        }

        public void StartSplitRes()
        {
            Debug.Assert(undecidedResPile.IsEmpty);

            targetStoredResAmounts = Industry switch
            {
                null => ResAmounts.Empty,
                not null => Industry.TargetStoredResAmounts()
            };

            // deal with resources
            state.StoredResPile.TransferAllTo(destin: undecidedResPile);
            state.waitingResAmountsPackets.ReturnAndRemove(destination: NodeID).TransferAllTo(destin: undecidedResPile);

            undecidedResPile.TransferUpTo(destin: state.StoredResPile, resAmounts: targetStoredResAmounts);
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
                undecidedResPile.TransferAllSingleResTo(destin: state.StoredResPile, resInd: resInd);
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResPile[resInd], maxAmountsFunc: maxExtraResFunc);

                {
                    var unsplitResPile = ReservedResPile.CreateIfHaveEnough(source: undecidedResPile, resAmount: new(resInd: resInd, amount: unsplitResAmount));
                    Debug.Assert(unsplitResPile is not null);
                    ReservedResPile.TransferAll
                    (
                        reservedSource: ref unsplitResPile,
                        destin: state.StoredResPile
                    );
                }

                foreach (var (destination, resAmountNum) in splitResAmounts)
                {
                    ResAmount resAmount = new(resInd: resInd, amount: resAmountNum);
                    var resPileForDestin = ReservedResPile.CreateIfHaveEnough(source: undecidedResPile, resAmount: resAmount);
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

#warning Continue here
                //deal with mass here
                //and in general, when stuff is added to planet/link, the function could take the source of the stuff to ensure that stuff actually leaves one
                //place and goes to the other
                resFirstLinks[(NodeID, destinationId)]!.TransferAll(start: this, resAmountsPacket: resAmountsPacket);
            }

            state.TooManyResStored = !(state.StoredResPile.ResAmounts <= targetStoredResAmounts);

            // TODO: look at this
            infoTextBox.Text = $"consists of {state.MainResAmount} {state.ConsistsOfResInd}\nstores {state.StoredResPile}\ntarget {targetStoredResAmounts}\n";

            // update text
            textBox.Text = CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd =>
                {
                    string text = "";
                    if (IfStore(resInd: resInd))
                        text += "store\n";
                    if (state.StoredResPile[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
                        text += (state.StoredResPile[resInd] >= targetStoredResAmounts[resInd]) switch
                        {
                            true => $"have {state.StoredResPile[resInd] - targetStoredResAmounts[resInd]} extra resources",
                            false => $"have {(double)state.StoredResPile[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                        };
                    return text;
                },
                allResCase: () =>
                {
                    ulong totalStoredMass = state.StoredResPile.Mass;
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

        void ILinkFacingPlanet.AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        void ILinkFacingPlanet.Arrive(ResAmountsPacketsByDestin resAmountsPackets)
        {
            state.RegisterArriving(hasMass: resAmountsPackets);
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeID);
            state.waitingResAmountsPackets.TransferAllFrom(sourcePackets: resAmountsPackets);
        }

        void ILinkFacingPlanet.Arrive(RealPeople people)
        {
            if (people.Count is 0)
                return;
            state.RegisterArriving(hasMass: people);
            state.WaitingPeople.TransferAllFrom(peopleSource: people);
        }

        void ILinkFacingPlanet.Arrive(RealPerson person, RealPeople personSource)
        {
            state.RegisterArriving(hasMass: person);
            state.WaitingPeople.TransferFrom(realPerson: person, personSource: personSource);
        }

        UDouble INodeAsLocalEnergyProducer.LocallyProducedWatts
            => Industry?.PeopleWorkOnTop switch
            {
                true or null => state.WattsHittingSurfaceOrIndustry * (UDouble).001,
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
            => state.WattsHittingSurfaceOrIndustry = 0;

        void ILightCatchingObject.SetWatts(StarID starPos, UDouble watts, Propor powerPropor)
            => state.WattsHittingSurfaceOrIndustry += watts;

        void INodeAsResDestin.AddResTravelHere(ResAmount resAmount)
            => resTravelHereAmounts = resTravelHereAmounts.WithAdd(resAmount: resAmount);
    }
}
