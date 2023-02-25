using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using Game1.Inhabitants;
using Microsoft.Xna.Framework;

namespace Game1
{
    [Serializable]
    public sealed class Planet : WorldUIElement, ILinkFacingPlanet, INodeAsLocalEnergyProducerAndConsumer, INodeAsResDestin, ILightCatchingObject, IWithRealPeopleStats
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
        public RealPeopleStats Stats { get; private set; }

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
        private ElectricalEnergy locallyProducedEnergy, usedLocalEnergy;

        private readonly TextBox textBox;
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
            undecidedResPile = ResPile.CreateEmpty(thermalBody: state.ThermalBody);
            resTravelHereAmounts = ResAmounts.Empty;
            usedLocalEnergy = ElectricalEnergy.zero;

            textBox = new(textColor: colorConfig.almostWhiteColor);
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
            foreach (var resInd in ResInd.All)
            {
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

            Mass startingNonPlanetMass = Mass.zero;
            // this is here beause it uses infoPanel, so that needs to be initialized first
            if (startingConditions is var (houseFactory, personCount, resSource))
            {
                // This is done so that buildings and people take stuff from this planet (i.e. MassCounter is of this planet)
                state.StoredResPile.TransferAllFrom(source: resSource);
                ResAmounts houseBuildingCost = houseFactory.BuildingCost(state: state);

                {
                    var reservedBuildingRes = ResPile.CreateIfHaveEnough
                    (
                        source: state.StoredResPile,
                        amount: houseBuildingCost
                    );
                    Debug.Assert(reservedBuildingRes is not null);
                    startingNonPlanetMass += reservedBuildingRes.Amount.Mass();
                    Building building = new(resSource: reservedBuildingRes);
                    Industry = (houseFactory as IFactoryForIndustryWithBuilding).CreateIndustry
                    (
                        state: state,
                        building: building
                    );
                }

                for (ulong i = 0; i < personCount; i++)
                {
                    RealPerson.GeneratePersonByMagic
                    (
                        nodeID: NodeID,
                        resSource: ResPile.CreateIfHaveEnough(source: state.StoredResPile, amount: RealPerson.resAmountsPerPerson)!,
                        childDestin: state.WaitingPeople
                    );
                }
                startingNonPlanetMass += state.WaitingPeople.Stats.totalMass;
                resSource.TransferAllFrom(source: state.StoredResPile);
            }
            else
                Industry = null;

            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public ulong TotalQueuedRes(ResInd resInd)
            => state.StoredResPile.Amount[resInd] + resTravelHereAmounts[resInd];

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

        public void Update(IReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks, EnergyPile<HeatEnergy> vacuumHeatPile)
        {
            // take people whose destination is this planet
            state.WaitingPeople.ForEach
            (
                personalAction: realPerson =>
                {
                    if (realPerson.ActivityCenterNodeID == NodeID)
                        realPerson.Arrived(realPersonSource: state.WaitingPeople);
                }
            );

            Industry = Industry?.Update();

            state.RadiantEnergyPile.TransformProporTo
            (
                destin: vacuumHeatPile,
                propor: Industry?.SurfaceReflectance ?? state.ConsistsOfRes.Reflectance,
                curTime: CurWorldManager.CurTime
            );

            state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: state.RadiantEnergyPile);

            state.ThermalBody.TransferHeatEnergyTo
            (
                destin: vacuumHeatPile,
                amount: Algorithms.HeatEnergyToDissipate
                (
                    heatEnergy: state.ThermalBody.HeatEnergy,
                    heatCapacity: state.ThermalBody.HeatCapacity,
                    surfaceLength: state.ApproxSurfaceLength,
                    emissivity: Industry?.SurfaceEmissivity ?? state.ConsistsOfRes.Emissivity,
                    stefanBoltzmannConstant: CurWorldConfig.stefanBoltzmannConstant,
                    temperatureExponent: CurWorldConfig.temperatureExponentInStefanBoltzmannLaw
                )
            );

            // MAKE sure that all resources (and people) leaving the planet do so AFTER the the temperature is established for that frame,
            // i.e. after appropriate amount of energy is radiated to space.

            // IF need to use current planet temperature for something, calculate it once per frame here, then use it.
            // Don't want to calculate temperature on the fly each time, as that would lead to higher temperatures at the beginning of the frame
            // due to getting heat energy from electricity used in links and industry.

            // transfer people who want to go to other places
            state.WaitingPeople.ForEach
            (
                personalAction: realPerson =>
                {
                    NodeID? activityCenterPosition = realPerson.ActivityCenterNodeID;
                    if (activityCenterPosition is null)
                        return;
                    Debug.Assert(activityCenterPosition != NodeID, "people who want to be here should have been taken already");
                    personFirstLinks[(NodeID, activityCenterPosition)]!.TransferFrom(start: this, realPersonSource: state.WaitingPeople, realPerson: realPerson);
                }
            );

            textBox.Shape.Center = state.Position;
        }

        public void UpdatePeople()
        {
            RealPerson.UpdateLocationParams personUpdateParams = new(LastNodeID: NodeID, ClosestNodeID: NodeID);
            Industry?.UpdatePeople(updateLocationParams: personUpdateParams);
            state.WaitingPeople.Update(updateLocationParams: personUpdateParams, updatePersonSkillsParams: null);
            Debug.Assert(state.LocationCounters.GetCount<NumPeople>() == state.WaitingPeople.NumPeople + (Industry?.Stats.totalNumPeople ?? NumPeople.zero));
            Stats = state.WaitingPeople.Stats.CombineWith(Industry?.Stats ?? RealPeopleStats.empty);
        }

        public void StartSplitRes()
        {
            Debug.Assert(undecidedResPile.IsEmpty);

            targetStoredResAmounts = Industry?.TargetStoredResAmounts() ?? ResAmounts.Empty;

            // deal with resources
            undecidedResPile.TransferAllFrom(source: state.StoredResPile);
            undecidedResPile.TransferAllFrom
            (
                source: state.waitingResAmountsPackets.ReturnAndRemove
                (
                    destination: NodeID
                )
            );

            state.StoredResPile.TransferAtMostFrom(source: undecidedResPile, maxAmount: targetStoredResAmounts);
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(Func<NodeID, INodeAsResDestin> nodeIDToNode, ResInd resInd, Func<NodeID, ulong> maxExtraResFunc)
        {
            if (undecidedResPile.Amount[resInd] is 0)
                return;

            var resSplitter = resSplittersToDestins[resInd];
            if (resSplitter.Empty)
                state.StoredResPile.TransferAllSingleResFrom(source: undecidedResPile, resInd: resInd);
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResPile.Amount[resInd], maxAmountsFunc: maxExtraResFunc);

                {
                    var unsplitResPile = ResPile.CreateIfHaveEnough(source: undecidedResPile, amount: new(resInd: resInd, amount: unsplitResAmount));
                    Debug.Assert(unsplitResPile is not null);
                    state.StoredResPile.TransferAllFrom(source: unsplitResPile);
                }

                foreach (var (destination, resAmountNum) in splitResAmounts)
                {
                    ResAmount resAmount = new(resInd: resInd, amount: resAmountNum);
                    var resPileForDestin = ResPile.CreateIfHaveEnough(source: undecidedResPile, amount: new(resAmount));
                    Debug.Assert(resPileForDestin is not null);
                    state.waitingResAmountsPackets.TransferAllFrom
                    (
                        source: resPileForDestin,
                        destination: destination
                    );
                    nodeIDToNode(destination).AddResTravelHere(resAmount: resAmount);
                }
            }
            Debug.Assert(undecidedResPile.Amount[resInd] == 0);
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

                resFirstLinks[(NodeID, destinationId)]!.TransferAllFrom(start: this, resAmountsPacket: resAmountsPacket);
            }

            state.TooManyResStored = !(state.StoredResPile.Amount <= targetStoredResAmounts);

            // TODO: look at this
            infoTextBox.Text = $"""
                consists of {state.MainResAmount} {state.ConsistsOfResInd}
                stores {state.StoredResPile}
                target {targetStoredResAmounts}
                Mass of everything {state.LocationCounters.GetCount<ResAmounts>().Mass}
                Mass of planet {state.PlanetMass}
                Number of people {state.LocationCounters.GetCount<NumPeople>()}

                travelling people stats:
                {state.WaitingPeople.Stats}

                """;

            // update text
            textBox.Text = CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd =>
                {
                    if (state.StoredResPile.Amount[resInd] is not 0 || targetStoredResAmounts[resInd] is not 0)
                        return (state.StoredResPile.Amount[resInd] >= targetStoredResAmounts[resInd]) switch
                        {
                            true => $"have {state.StoredResPile.Amount[resInd] - targetStoredResAmounts[resInd]} extra resources",
                            false => $"have {(double)state.StoredResPile.Amount[resInd] / targetStoredResAmounts[resInd] * 100:0.}% of target stored resources\n",
                        };
                    else
                        return "";
                },
                allResCase: () =>
                {
                    Mass totalStoredMass = state.StoredResPile.Amount.Mass();
                    return totalStoredMass.IsZero switch
                    {
                        true => "",
                        false => $"stored total res mass {totalStoredMass}"
                    };
                },
                powerCase: () => "",
                peopleCase: () => ""
            ).Trim();

            infoTextBox.Text += textBox.Text;
            infoTextBox.Text = infoTextBox.Text.Trim();
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

        UDouble ILinkFacingPlanet.SurfaceGravity
            => state.SurfaceGravity;

        void ILinkFacingPlanet.AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        void ILinkFacingPlanet.Arrive(ResAmountsPacketsByDestin resAmountsPackets)
        {
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeID);
            state.waitingResAmountsPackets.TransferAllFrom(sourcePackets: resAmountsPackets);
        }

        void ILinkFacingPlanet.Arrive(RealPeople realPeople)
        {
            if (realPeople.NumPeople.IsZero)
                return;
            state.WaitingPeople.TransferAllFrom(realPeopleSource: realPeople);
        }

        void ILinkFacingPlanet.Arrive(RealPerson realPerson, RealPeople realPersonSource)
            => state.WaitingPeople.TransferFrom(realPerson: realPerson, realPersonSource: realPersonSource);

        void ILinkFacingPlanet.TransformAllElectricityToHeatAndTransferFrom(EnergyPile<ElectricalEnergy> source)
            => state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: source);

        void INodeAsLocalEnergyProducerAndConsumer.ProduceLocalEnergy(EnergyPile<ElectricalEnergy> destin)
        {
            if (Industry?.PeopleWorkOnTop is true or null)
            {
                locallyProducedEnergy = state.RadiantEnergyPile.TransformProporTo
                (
                    destin: destin,
                    propor: CurWorldConfig.planetTransformRadiantToElectricalEnergyPropor,
                    curTime: CurWorldManager.CurTime
                );
            }
            else
                locallyProducedEnergy = ElectricalEnergy.zero;
        }

        private ILightBlockingObject CurLightCatchingObject
            => Industry?.LightBlockingObject ?? shape;

        AngleArc.Params ILightBlockingObject.BlockedAngleArcParams(MyVector2 lightPos)
            => CurLightCatchingObject.BlockedAngleArcParams(lightPos: lightPos);

        double ILightBlockingObject.CloserInterPoint(MyVector2 lightPos, MyVector2 lightDir)
            => CurLightCatchingObject.CloserInterPoint(lightPos: lightPos, lightDir: lightDir);

        void ILightCatchingObject.TakeRadiantEnergyFrom(EnergyPile<RadiantEnergy> source, RadiantEnergy amount)
            => state.RadiantEnergyPile.TransferFrom(source: source, amount: amount);

        void INodeAsResDestin.AddResTravelHere(ResAmount resAmount)
            => resTravelHereAmounts = resTravelHereAmounts.WithAdd(resAmount: resAmount);

        void INodeAsLocalEnergyProducerAndConsumer.ConsumeUnusedLocalEnergyFrom(EnergyPile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: source);
            usedLocalEnergy = locallyProducedEnergy - electricalEnergy;
        }
    }
}
