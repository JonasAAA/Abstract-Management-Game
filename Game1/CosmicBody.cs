using Game1.Delegates;
using Game1.Industries;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using Game1.Inhabitants;
using Game1.Collections;

namespace Game1
{
    [Serializable]
    public sealed class CosmicBody : WorldUIElement, ILightSource, ILinkFacingCosmicBody, INodeAsLocalEnergyProducerAndConsumer, INodeAsResDestin, ILightCatchingObject, IWithRealPeopleStats
    {
        [Serializable]
        private readonly record struct ResDesinArrowEventListener(CosmicBody Node, IResource Res) : IDeletedListener, INumberChangedListener
        {
            public void SyncSplittersWithArrows()
            {
                var resCopy = Res;
                List<ResDestinArrow> relevantResDistribArrows = Node.resDistribArrows.Where(resDistribArrow => resDistribArrow.res == resCopy).ToList();
                foreach (var resDestinArrow in relevantResDistribArrows)
                    Node.resSplittersToDestins.GetOrCreate(key: Res).SetImportance
                    (
                        key: resDestinArrow.DestinationId,
                        importance: (ulong)resDestinArrow.Importance
                    );
                int totalImportance = relevantResDistribArrows.Sum(resDestinArrow => resDestinArrow.Importance);
                foreach (var resDestinArrow in relevantResDistribArrows)
                    resDestinArrow.TotalImportance = totalImportance;
            }

            void IDeletedListener.DeletedResponse(IDeletable deletable)
            {
                if (deletable is ResDestinArrow resDestinArrow)
                {
                    Node.resDistribArrows.RemoveChild(child: resDestinArrow);
                    CurWorldManager.RemoveResDestinArrow(resDestinArrow: resDestinArrow);
                    Node.resSplittersToDestins.GetOrCreate(key: Res).RemoveKey(key: resDestinArrow.DestinationId);
                    SyncSplittersWithArrows();
                }
                else
                    throw new ArgumentException();
            }

            void INumberChangedListener.NumberChangedResponse()
                => SyncSplittersWithArrows();
        }

        [Serializable]
        private readonly record struct BuildIndustryButtonClickedListener(CosmicBody Node, IBuildableFactory BuildableParams) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => Node.Industry = BuildableParams.CreateIndustry(state: Node.state);
        }

        //[Serializable]
        //private readonly record struct AddResourceDestinationButtonClickedListener : IClickedListener
        //{
        //    void IClickedListener.ClickedResponse()
        //        => CurWorldManager.ArrowDrawingModeOn = true;
        //}

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

        private IIndustry? Industry
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
        private readonly LightPolygon lightPolygon;
        private readonly List<Link> links;
        /// <summary>
        /// NEVER use this directly, use Planet.Industry instead
        /// </summary>
        private IIndustry? industry;
        private readonly AutoCreateValDict<IResource, HistoricProporSplitter<NodeID>> resSplittersToDestins;
        private SomeResAmounts<IResource> targetStoredResAmounts;
        private readonly ResPile undecidedResPile;
        private AllResAmounts resTravelHereAmounts;
        private readonly new LightCatchingDisk shape;
        private ElectricalEnergy locallyProducedEnergy, usedLocalEnergy;
        private readonly SimpleHistoricProporSplitter<IRadiantEnergyConsumer> radiantEnergySplitter;
        private readonly HistoricRounder energyToDissipateRounder, heatEnergyToDissipateRounder, massFusionRounder, reflectedRadiantEnergyRounder, capturedForUseRadiantEnergyRounder;
        private readonly EnergyPile<RadiantEnergy> radiantEnergyToDissipatePile;
        private RadiantEnergy radiantEnergyToDissipate;

        /// <summary>
        /// This is current temperature to be used until the new value is calculated.
        /// Don't calculate temperature on the fly each time, as that would lead to temperature variations during the frame.
        /// </summary>
        private UDouble temperatureInK;
        private ulong matterCountConvertedToEnergy;

        private readonly TextBox textBox;
        private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        private readonly UIRectPanel<IHUDElement> infoPanel, buildButtonPannel;
        //private readonly MyDict<IOverlay, UIRectPanel<IHUDElement>> overlayTabPanels;
        private readonly TextBox infoTextBox;
        //private readonly string overlayTabLabel;
        private readonly UITransparentPanel<ResDestinArrow> resDistribArrows;

        public CosmicBody(NodeState state, Color activeColor)
            //, (IFactoryForIndustryWithBuilding industryFactory, ulong personCount, ResPile resSource)? startingConditions = null)
            : base
            (
                shape: new LightCatchingDisk(parameters: new ShapeParams(State: state)),
                activeColor: activeColor,
                inactiveColor: state.ConsistsOf.color,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            lightPolygon = new(color: state.ConsistsOf.color);
            shape = (LightCatchingDisk)base.shape;

            links = new();

            resSplittersToDestins = new();
            //selector: res => new HistoricProporSplitter<NodeID>()
            targetStoredResAmounts = SomeResAmounts<IResource>.empty;
            undecidedResPile = ResPile.CreateEmpty(thermalBody: state.ThermalBody);
            resTravelHereAmounts = AllResAmounts.empty;
            usedLocalEnergy = ElectricalEnergy.zero;
            radiantEnergySplitter = new();
            energyToDissipateRounder = new();
            heatEnergyToDissipateRounder = new();
            massFusionRounder = new();
            reflectedRadiantEnergyRounder = new();
            capturedForUseRadiantEnergyRounder = new();
            radiantEnergyToDissipatePile = EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: state.LocationCounters);
            radiantEnergyToDissipate = RadiantEnergy.zero;
#warning have a config parameter for that
            temperatureInK = 100;
            matterCountConvertedToEnergy = 0;

            textBox = new(textColor: colorConfig.almostWhiteColor);
            textBox.Shape.MinWidth = 100;
            UpdateHUDPos();
            CurWorldManager.AddWorldHUDElement(worldHUDElement: textBox);

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

            //overlayTabLabel = "overlay tab";
            //overlayTabPanels = new();

            //foreach (var overlay in IOverlay.all)
            //    overlayTabPanels[overlay] = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
            //foreach (var res in Res.All)
            //{
            //    Button addResourceDestinationButton = new
            //    (
            //        shape: new MyRectangle(width: 150, height: 50),
            //        tooltip: new ImmutableTextTooltip(text: $"Adds new place to where {res} should be transported"),
            //        text: $"add resource {res}\ndestination"
            //    );
            //    addResourceDestinationButton.clicked.Add(listener: new AddResourceDestinationButtonClickedListener());

            //    overlayTabPanels[res].AddChild
            //    (
            //        child: addResourceDestinationButton
            //    );
            //}
            //UITabs.Add
            //((
            //    tabLabelText: overlayTabLabel,
            //    tabTooltip: new ImmutableTextTooltip(text: "UI specific to the current overlay"),
            //    tab: overlayTabPanels[CurWorldManager.Overlay]
            //));

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                tabs: UITabs
            );

            Popup = UITabPanel;

            //SetPopup
            //(
            //    HUDElement: UITabPanel,
            //    overlays: IOverlay.all
            //);

            resDistribArrows = new();

            //Mass startingNonPlanetMass = Mass.zero;
            //// this is here beause it uses infoPanel, so that needs to be initialized first
            //if (startingConditions is var (industryFactory, personCount, resSource))
            //{
            //    // This is done so that buildings and people take stuff from this planet (i.e. MassCounter is of this planet)
            //    state.StoredResPile.TransferAllFrom(source: resSource);
            //    ResAmounts houseBuildingCost = industryFactory.BuildingCost(state: state);

            //    {
            //        var reservedBuildingRes = ResPile.CreateIfHaveEnough
            //        (
            //            source: state.StoredResPile,
            //            amount: houseBuildingCost
            //        );
            //        Debug.Assert(reservedBuildingRes is not null);
            //        startingNonPlanetMass += reservedBuildingRes.Amount.Mass();
            //        Building building = new(resSource: reservedBuildingRes);
            //        Industry = industryFactory.CreateIndustry
            //        (
            //            state: state,
            //            building: building
            //        );
            //    }

            //    for (ulong i = 0; i < personCount; i++)
            //    {
            //        RealPerson.GeneratePersonByMagic
            //        (
            //            closestNodeID: NodeID,
            //            resSource: ResPile.CreateIfHaveEnough(source: state.StoredResPile, amount: RealPerson.resAmountsPerPerson)!,
            //            childDestin: state.WaitingPeople
            //        );
            //    }
            //    startingNonPlanetMass += state.WaitingPeople.Stats.totalMass;
            //    resSource.TransferAllFrom(source: state.StoredResPile);
            //}
            //else
            //    Industry = null;

            CurWorldManager.AddLightSource(lightSource: this);
            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public void UpdateHUDPos()
            => textBox.Shape.Center = CurWorldManager.WorldPosToHUDPos(worldPos: Position);

        public ulong TotalQueuedRes(IResource res)
            => state.StoredResPile.Amount.resAmounts[res] + resTravelHereAmounts.resAmounts[res];

        public IEnumerable<NodeID> ResDestins(IResource res)
            => resSplittersToDestins.GetOrCreate(key: res).Keys;

        public ulong TargetStoredResAmount(IResource res)
            => targetStoredResAmounts[res];
        
        public bool CanHaveDestin(NodeID destinationId, IResource res)
        {
            if (!Active || CurWorldManager.ArrowDrawingModeRes is null)
                throw new InvalidOperationException();

            return destinationId != NodeID && !resSplittersToDestins.GetOrCreate(key: res).ContainsKey(destinationId);
        }

        public void AddResDestin(NodeID destinationId, IResource res)
        {
            if (!CanHaveDestin(destinationId: destinationId, res: res))
                throw new ArgumentException();

            if (resSplittersToDestins.GetOrCreate(key: res).ContainsKey(key: destinationId))
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
                res: res
            );
            ResDesinArrowEventListener resDesinArrowEventListener = new(Node: this, Res: res);
            resDestinArrow.ImportanceNumberChanged.Add(listener: resDesinArrowEventListener);
            resDestinArrow.Deleted.Add(listener: resDesinArrowEventListener);

            resDistribArrows.AddChild(child: resDestinArrow);
            CurWorldManager.AddResDestinArrow(resDestinArrow: resDestinArrow);
            resDesinArrowEventListener.SyncSplittersWithArrows();
        }

        public void Update(EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks, EnergyPile<HeatEnergy> vacuumHeatEnergyPile)
        {
            Industry = Industry?.Update();

            // take people whose destination is this planet
            state.WaitingPeople.ForEach
            (
                personalAction: realPerson =>
                {
                    if (realPerson.ActivityCenterNodeID == NodeID)
                        realPerson.Arrived(realPersonSource: state.WaitingPeople);
                }
            );

            state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: state.RadiantEnergyPile);

            matterCountConvertedToEnergy = Algorithms.MatterToConvertToEnergy
            (
                rawMat: state.ConsistsOf,
                resAmount: state.MainResAmount,
                temperatureInK: temperatureInK,
                surfaceGravity: state.SurfaceGravity,
                duration: CurWorldManager.Elapsed,
                massInKgRoundFunc: mass => massFusionRounder.Round(value: mass, curTime: CurWorldManager.CurTime),
                reactionStrengthCoeff: CurWorldConfig.reactionStrengthCoeff,
                nonConvertedMassForUnitReactionStrengthUnitTime: CurWorldConfig.nonConvertedMassForUnitReactionStrengthUnitTime
            );

            state.consistsOfResPile.TransformResToHeatEnergy
            (
                amount: AllResAmounts.CreateFromOnlyMix
                (
                    rawMatsMix: new
                    (
                        res: state.ConsistsOf,
                        amount: matterCountConvertedToEnergy
                    )
                )
            );

            state.RecalculateValues();

            (HeatEnergy heatEnergyToDissipate, radiantEnergyToDissipate) = Algorithms.EnergiesToDissipate
            (
                heatEnergy: state.ThermalBody.HeatEnergy,
                surfaceLength: state.ApproxSurfaceLength,
                emissivity: Industry?.SurfaceEmissivity ?? state.ConsistsOf.Emissivity,
                temperatureInK: temperatureInK,
                energyInJToDissipateRoundFunc: energyInJ => energyToDissipateRounder.Round(value: energyInJ, curTime: CurWorldManager.CurTime),
                stefanBoltzmannConstant: CurWorldConfig.stefanBoltzmannConstant,
                temperatureExponent: CurWorldConfig.temperatureExponentInStefanBoltzmannLaw,
                heatEnergyInJRoundFunc: heatEnergyInJ => heatEnergyToDissipateRounder.Round(value: heatEnergyInJ, curTime: CurWorldManager.CurTime),
                allHeatMaxTemper: CurWorldConfig.allHeatMaxTemper,
                halfHeatTemper: CurWorldConfig.halfHeatTemper,
                heatEnergyDropoffExponent: CurWorldConfig.heatEnergyDropoffExponent
            );

            state.ThermalBody.TransferHeatEnergyTo
            (
                destin: vacuumHeatEnergyPile,
                amount: heatEnergyToDissipate
            );

            state.ThermalBody.TransformHeatEnergyTo
            (
                destin: radiantEnergyToDissipatePile,
                amount: radiantEnergyToDissipate
            );

            temperatureInK = (UDouble)state.ThermalBody.HeatEnergy.ValueInJ() / state.ThermalBody.HeatCapacity.valueInJPerK;

            // MAKE sure that all resources (and people) leaving the planet do so AFTER the the temperatureInK is established for that frame,
            // i.e. after appropriate amount of energy is radiated to space.

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

            UpdateHUDPos();

            Debug.Assert(state.RadiantEnergyPile.Amount.IsZero);
        }

        //public void UpdatePeople()
        //{
        //    Industry?.UpdatePeople();
        //    state.WaitingPeople.Update(updatePersonSkillsParams: null);
        //    Debug.Assert(state.LocationCounters.GetCount<NumPeople>() == state.WaitingPeople.NumPeople + (Industry?.Stats.totalNumPeople ?? NumPeople.zero));
        //    Stats = state.WaitingPeople.Stats.CombineWith(Industry?.Stats ?? RealPeopleStats.empty);
        //}

        public void StartSplitRes()
        {
            Debug.Assert(undecidedResPile.IsEmpty);

            targetStoredResAmounts = Industry?.TargetStoredResAmounts() ?? SomeResAmounts<IResource>.empty;

            // deal with resources
            undecidedResPile.TransferAllFrom(source: state.StoredResPile);
            undecidedResPile.TransferAllFrom
            (
                source: state.waitingResAmountsPackets.ReturnAndRemove
                (
                    destination: NodeID
                )
            );

            state.StoredResPile.TransferAtMostFrom
            (
                source: undecidedResPile,
                maxAmount: AllResAmounts.CreateFromNoMix
                (
                    resAmounts: targetStoredResAmounts
                )
            );
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(Func<NodeID, INodeAsResDestin> nodeIDToNode, IResource res, Func<NodeID, ulong> maxExtraResFunc)
        {
            if (undecidedResPile.Amount.resAmounts[res] is 0)
                return;

            var resSplitter = resSplittersToDestins.GetOrCreate(key: res);
            if (resSplitter.Empty)
                state.StoredResPile.TransferAllSingleResFrom(source: undecidedResPile, res: res);
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResPile.Amount.resAmounts[res], maxAmountsFunc: maxExtraResFunc);

                {
                    var unsplitResPile = ResPile.CreateIfHaveEnough
                    (
                        source: undecidedResPile,
                        amount: AllResAmounts.CreateFromNoMix
                        (
                            resAmounts: new
                            (
                                res: res,
                                amount: unsplitResAmount
                            )
                        )
                    );
                    Debug.Assert(unsplitResPile is not null);
                    state.StoredResPile.TransferAllFrom(source: unsplitResPile);
                }

                foreach (var (destination, resAmountNum) in splitResAmounts)
                {
                    ResAmount<IResource> resAmount = new(res: res, amount: resAmountNum);
                    var resPileForDestin = ResPile.CreateIfHaveEnough
                    (
                        source: undecidedResPile,
                        amount: AllResAmounts.CreateFromNoMix
                        (
                            resAmounts: new(resAmount: resAmount)
                        )
                    );
                    Debug.Assert(resPileForDestin is not null);
                    state.waitingResAmountsPackets.TransferAllFrom
                    (
                        source: resPileForDestin,
                        destination: destination
                    );
                    nodeIDToNode(destination).AddResTravelHere(resAmount: resAmount);
                }
            }
            Debug.Assert(undecidedResPile.Amount.resAmounts[res] is 0);
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes(EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> resFirstLinks)
        {
            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeID destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeID);

                resFirstLinks[(NodeID, destinationId)]!.TransferAllFrom(start: this, resAmountsPacket: resAmountsPacket);
            }

            throw new NotImplementedException();
            //state.TooManyResStored = !(state.StoredResPile.Amount <= targetStoredResAmounts);

            // TODO: look at this
            infoTextBox.Text = $"""
                consists of {state.MainResAmount} {state.ConsistsOf}
                stores {state.StoredResPile}
                target {targetStoredResAmounts}
                Mass of everything {state.LocationCounters.GetCount<AllResAmounts>().Mass()}
                Mass of planet {state.PlanetMass}
                Number of people {state.LocationCounters.GetCount<NumPeople>()}

                travelling people stats:
                {state.WaitingPeople.Stats}

                """;

            // update text
            //textBox.Text = CurWorldManager.Overlay.SwitchExpression
            //(
            //    singleResCase: res =>
            //    {
            //        if (state.StoredResPile.Amount[res] is not 0 || targetStoredResAmounts[res] is not 0)
            //            return (state.StoredResPile.Amount[res] >= targetStoredResAmounts[res]) switch
            //            {
            //                true => $"have {state.StoredResPile.Amount[res] - targetStoredResAmounts[res]} extra resources",
            //                false => $"have {(double)state.StoredResPile.Amount[res] / targetStoredResAmounts[res] * 100:0.}% of target stored resources\n",
            //            };
            //        else
            //            return "";
            //    },
            //    allResCase: () =>
            //    {
            //        Mass totalStoredMass = state.StoredResPile.Amount.Mass();
            //        return totalStoredMass.IsZero switch
            //        {
            //            true => "",
            //            false => $"stored total res mass {totalStoredMass}"
            //        };
            //    },
            //    powerCase: () => "",
            //    peopleCase: () => ""
            //);

            textBox.Text += $"T = {temperatureInK:0.} K\nM to E = {matterCountConvertedToEnergy}\n";
            textBox.Text = textBox.Text.Trim();

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

            if (Active && CurWorldManager.ArrowDrawingModeRes is not null)
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

        //public override void ChoiceChangedResponse(IOverlay prevOverlay)
        //{
        //    base.ChoiceChangedResponse(prevOverlay: prevOverlay);

        //    UITabPanel.ReplaceTab
        //        (
        //            tabLabelText: overlayTabLabel,
        //            tab: overlayTabPanels[CurWorldManager.Overlay]
        //        );
        //}

        UDouble ILinkFacingCosmicBody.SurfaceGravity
            => state.SurfaceGravity;

        void ILinkFacingCosmicBody.AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        void ILinkFacingCosmicBody.Arrive(ResAmountsPacketsByDestin resAmountsPackets)
        {
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: NodeID);
            state.waitingResAmountsPackets.TransferAllFrom(sourcePackets: resAmountsPackets);
        }

        void ILinkFacingCosmicBody.ArriveAndDeleteSource(RealPeople realPeopleSource)
            => state.WaitingPeople.TransferAllFromAndDeleteSource(realPeopleSource: realPeopleSource);

        void ILinkFacingCosmicBody.Arrive(RealPerson realPerson, RealPeople realPersonSource)
            => state.WaitingPeople.TransferFrom(realPerson: realPerson, realPersonSource: realPersonSource);

        void ILinkFacingCosmicBody.TransformAllElectricityToHeatAndTransferFrom(EnergyPile<ElectricalEnergy> source)
            => state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: source);

        void INodeAsLocalEnergyProducerAndConsumer.ProduceLocalEnergy(EnergyPile<ElectricalEnergy> destin)
        {
            locallyProducedEnergy = state.RadiantEnergyPile.TransformProporTo
            (
                destin: destin,
                propor: CurWorldConfig.planetTransformRadiantToElectricalEnergyPropor,
                amountToTransformRoundFunc: amount => capturedForUseRadiantEnergyRounder.Round(value: amount, curTime: CurWorldManager.CurTime)
            );

            //if (Industry?.PeopleWorkOnTop is true or null)
            //{
            //    locallyProducedEnergy = state.RadiantEnergyPile.TransformProporTo
            //    (
            //        destin: destin,
            //        propor: CurWorldConfig.planetTransformRadiantToElectricalEnergyPropor,
            //        amountToTransformRoundFunc: amount => capturedForUseRadiantEnergyRounder.Round(value: amount, curTime: CurWorldManager.CurTime)
            //    );
            //}
            //else
            //    locallyProducedEnergy = ElectricalEnergy.zero;
        }

        private ILightBlockingObject CurLightCatchingObject
            => Industry?.LightBlockingObject ?? shape;

        AngleArc.Params ILightBlockingObject.BlockedAngleArcParams(MyVector2 lightPos)
            => CurLightCatchingObject.BlockedAngleArcParams(lightPos: lightPos);

        void IRadiantEnergyConsumer.TakeRadiantEnergyFrom(EnergyPile<RadiantEnergy> source, RadiantEnergy amount)
            => state.RadiantEnergyPile.TransferFrom(source: source, amount: amount);

        void IRadiantEnergyConsumer.EnergyTakingComplete(IRadiantEnergyConsumer vacuumAsRadiantEnergyConsumer)
            => vacuumAsRadiantEnergyConsumer.TakeRadiantEnergyFrom
            (
                source: state.RadiantEnergyPile,
                amount: Algorithms.EnergyPropor
                (
                    wholeAmount: state.RadiantEnergyPile.Amount,
                    propor: Industry?.SurfaceReflectance ?? state.ConsistsOf.Reflectance,
                    roundFunc: amount => reflectedRadiantEnergyRounder.Round
                    (
                        value: amount,
                        curTime: CurWorldManager.CurTime
                    )
                )
            );

        void INodeAsResDestin.AddResTravelHere(ResAmount<IResource> resAmount)
            => resTravelHereAmounts += AllResAmounts.CreateFromNoMix
            (
                resAmounts: new(resAmount: resAmount)
            );

        void INodeAsLocalEnergyProducerAndConsumer.ConsumeUnusedLocalEnergyFrom(EnergyPile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: source);
            usedLocalEnergy = locallyProducedEnergy - electricalEnergy;
        }

        // The complexity is O(N log N) where N is lightCatchingObjects.Count
        void ILightSource.ProduceAndDistributeRadiantEnergy(List<ILightCatchingObject> lightCatchingObjects, IRadiantEnergyConsumer vacuumAsRadiantEnergyConsumer)
        {
            if (radiantEnergyToDissipatePile.Amount.IsZero)
                return;
            // Removed in oder to not catch the radiant energy from itself
            bool isInLightCatchingObjects = lightCatchingObjects.Remove(this);

            RadiantEnergy producedRadiantEnergy = radiantEnergyToDissipatePile.Amount;

            GetAnglesAndBlockedAngleArcs
            (
                angles: out List<double> angles,
                blockedAngleArcs: out List<(bool start, AngleArc angleArc)> blockedAngleArcs
            );

            PrepareAngles(ref angles);

            blockedAngleArcs.Sort
            (
                comparison: (angleArc1, angleArc2)
                    => angleArc1.angleArc.GetAngle(start: angleArc1.start)
                    .CompareTo(angleArc2.angleArc.GetAngle(start: angleArc2.start))
            );

            CalculateLightPolygonAndRayCatchingObjects
            (
                vertices: out List<MyVector2> vertices,
                rayCatchingObjects: out List<ILightCatchingObject?> rayCatchingObjects
            );

            Debug.Assert(rayCatchingObjects.Count == angles.Count && vertices.Count == angles.Count);

            lightPolygon.Update
            (
                strength: state.Radius / CurWorldConfig.standardStarRadius,
                center: state.Position,
                vertices: vertices
            );

            DistributeStarPower(usedArc: out UDouble usedArc);

#warning Add that info to the text text box
            // popupTextBox.Text = $"generates {producedRadiantEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds} power\n{usedArc / (2 * MyMathHelper.pi) * 100:0.}% of it hits planets";

            if (isInLightCatchingObjects)
                lightCatchingObjects.Add(this);

            return;

            void GetAnglesAndBlockedAngleArcs(out List<double> angles, out List<(bool start, AngleArc angleArc)> blockedAngleArcs)
            {
                angles = new();
                blockedAngleArcs = new();
                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    var blockedAngleArc = lightCatchingObject.BlockedAngleArc(lightPos: state.Position);

                    angles.Add(blockedAngleArc.startAngle);
                    angles.Add(blockedAngleArc.endAngle);

                    if (blockedAngleArc.startAngle <= blockedAngleArc.endAngle)
                        addProperAngleArc(blockedAngleArcs: blockedAngleArcs, angleArc: blockedAngleArc);
                    else
                    {
                        addProperAngleArc
                        (
                            blockedAngleArcs: blockedAngleArcs,
                            angleArc: new
                            (
                                startAngle: blockedAngleArc.startAngle - 2 * MyMathHelper.pi,
                                endAngle: blockedAngleArc.endAngle,
                                radius: blockedAngleArc.radius,
                                lightCatchingObject: blockedAngleArc.lightCatchingObject
                            )
                        );
                        addProperAngleArc
                        (
                            blockedAngleArcs: blockedAngleArcs,
                            angleArc: new
                            (
                                startAngle: blockedAngleArc.startAngle,
                                endAngle: blockedAngleArc.endAngle + 2 * MyMathHelper.pi,
                                radius: blockedAngleArc.radius,
                                lightCatchingObject: blockedAngleArc.lightCatchingObject
                            )
                        );
                    }
                }
                void addProperAngleArc(List<(bool start, AngleArc angleArc)> blockedAngleArcs, AngleArc angleArc)
                {
                    blockedAngleArcs.Add((start: true, angleArc));
                    blockedAngleArcs.Add((start: false, angleArc));
                }
            }

            void PrepareAngles(ref List<double> angles)
            {
                // TODO: move to constants file
                const double small = .0001;
                int oldAngleCount = angles.Count;
                List<double> newAngles = new(2 * angles.Count);

                foreach (var angle in angles)
                {
                    newAngles.Add(angle - small);
                    newAngles.Add(angle + small);
                }

                for (int i = 0; i < 4; i++)
                    newAngles.Add(i * 2 * MyMathHelper.pi / 4);

                for (int i = 0; i < newAngles.Count; i++)
                    newAngles[i] = MyMathHelper.WrapAngle(angle: newAngles[i]);

                newAngles.Sort();
                angles = newAngles;
            }

            void CalculateLightPolygonAndRayCatchingObjects(out List<MyVector2> vertices, out List<ILightCatchingObject?> rayCatchingObjects)
            {
                vertices = new();
                rayCatchingObjects = new();
                // TODO: consider moving this to constants class
                UDouble maxDist = 2000 * CurWorldConfig.metersPerStartingPixel;

                SortedSet<AngleArc> curAngleArcs = new();
                int angleInd = 0, angleArcInd = 0;
                while (angleInd < angles.Count)
                {
                    double curAngle = angles[angleInd];
                    while (angleArcInd < blockedAngleArcs.Count)
                    {
                        var (curStart, curAngleArc) = blockedAngleArcs[angleArcInd];
                        if (curAngleArc.GetAngle(start: curStart) >= curAngle)
                            break;
                        if (curStart)
                            curAngleArcs.Add(curAngleArc);
                        else
                            curAngleArcs.Remove(curAngleArc);
                        angleArcInd++;
                    }

                    MyVector2 rayDir = MyMathHelper.Direction(rotation: curAngle);
                    rayCatchingObjects.Add(curAngleArcs.Count == 0 ? null : curAngleArcs.Min.lightCatchingObject);
                    double minDist = rayCatchingObjects[^1] switch
                    {
                        null => maxDist,
                        // adding 1 looks better, even though it's not needed mathematically
                        // TODO: move the constant 1 to the constants file
                        _ => 1 + curAngleArcs.Min.radius
                    };
                    vertices.Add(state.Position + minDist * rayDir);

                    angleInd++;
                }
            }

            void DistributeStarPower(out UDouble usedArc)
            {
                Dictionary<IRadiantEnergyConsumer, UDouble> arcsForObjects = lightCatchingObjects.ToDictionary
                (
                    keySelector: lightCatchingObject => lightCatchingObject as IRadiantEnergyConsumer,
                    elementSelector: lightCatchingObject => UDouble.zero
                );
                arcsForObjects.Add(key: vacuumAsRadiantEnergyConsumer, value: 0);
                usedArc = 0;
                for (int i = 0; i < rayCatchingObjects.Count; i++)
                {
                    UDouble curArc = MyMathHelper.Abs(MyMathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count]));
                    UseArc(rayCatchingObject: rayCatchingObjects[i], usedArc: ref usedArc);
                    UseArc(rayCatchingObject: rayCatchingObjects[(i + 1) % rayCatchingObjects.Count], usedArc: ref usedArc);

                    void UseArc(ILightCatchingObject? rayCatchingObject, ref UDouble usedArc)
                    {
                        if (rayCatchingObject is null)
                            arcsForObjects[vacuumAsRadiantEnergyConsumer] += curArc / 2;
                        else
                        {
                            arcsForObjects[rayCatchingObject] += curArc / 2;
                            usedArc += curArc / 2;
                        }
                    }
                }

                Debug.Assert(arcsForObjects.Values.Sum().IsCloseTo(other: 2 * MyMathHelper.pi));

                Dictionary<IRadiantEnergyConsumer, ulong> splitAmounts = radiantEnergySplitter.Split
                (
                    amount: producedRadiantEnergy.ValueInJ,
                    importances: arcsForObjects
                );

                foreach (var (radiantEnergyConsumer, allocAmount) in splitAmounts)
                    radiantEnergyConsumer.TakeRadiantEnergyFrom(source: radiantEnergyToDissipatePile, amount: RadiantEnergy.CreateFromJoules(valueInJ: allocAmount));
                Debug.Assert(radiantEnergyToDissipatePile.Amount.IsZero);
            }
        }

        void ILightSource.Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
        {
            if (radiantEnergyToDissipate.IsZero)
                return;
            lightPolygon.Draw
            (
                worldToScreenTransform: worldToScreenTransform,
                basicEffect: basicEffect,
                actualScreenWidth: actualScreenWidth,
                actualScreenHeight: actualScreenHeight
            );
        }
    }
}
