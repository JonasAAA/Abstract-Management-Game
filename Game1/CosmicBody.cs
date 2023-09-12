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
    public sealed class CosmicBody : WorldUIElement, ILightSource, ILinkFacingCosmicBody, INodeAsLocalEnergyProducerAndConsumer, ILightCatchingObject, IWithSpecialPositions, IWithRealPeopleStats
    {
        [Serializable]
        private readonly record struct ShapeParams(NodeState State) : Disk.IParams
        {
            public MyVector2 Center
                => State.Position;

            public UDouble Radius
                => State.Radius;
        }

        [Serializable]
        private sealed record TextBoxHUDPosUpdater(CosmicBody CosmicBody) : IAction
        {
            void IAction.Invoke()
                => CosmicBody.textBox.Shape.Center = CurWorldManager.WorldPosToHUDPos(worldPos: CosmicBody.Position);
        }

        public NodeID NodeID
            => state.NodeID;
        public MyVector2 Position
            => state.Position;
        public bool HasIndustry
            => industry is not null;
        public IIndustryFacingNodeState NodeState
            => state;
        public RealPeopleStats Stats { get; private set; }

        public IIndustry? Industry
        {
            get => industry;
            private set
            {
                if (industry == value)
                    return;

                infoPanel.RemoveChild(child: industry?.UIElement);
                industry = value;
                if (industry is not null)
                    infoPanel.AddChild(child: industry.UIElement);
            }
        }

        protected sealed override EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> Popups
        {
            get
            {
                List<(IHUDElement popup, IAction popupHUDPosUpdater)> popups = new(capacity: 2)
                {
                    (
                        popup: UITabPanel,
                        popupHUDPosUpdater: UITabPanelHUDPosUpdater
                    )
                };
                if (Industry is not null)
                {
                    var routePanel = Industry.RoutePanel;
                    popups.Add
                    (
                        (
                            popup: routePanel,
                            popupHUDPosUpdater: new HUDElementPosUpdater
                            (
                                HUDElement: routePanel,
                                baseWorldObject: this,
                                HUDElementOrigin: new(HorizPosEnum.Right, VertPosEnum.Middle),
                                anchorInBaseWorldObject: new(HorizPosEnum.Left, VertPosEnum.Middle)
                            )
                        )
                    );
                }
                return popups.ToEfficientReadOnlyCollection();
            }
        }
        protected sealed override Color Color
            => state.Composition.Color();

        private readonly NodeState state;
        private readonly LightPolygon lightPolygon;
        private readonly List<Link> links;
        /// <summary>
        /// NEVER use this directly, use Planet.Industry instead
        /// </summary>
        private IIndustry? industry;
        private readonly new LightBlockingDisk shape;
        private ElectricalEnergy locallyProducedEnergy, usedLocalEnergy;
        private readonly SimpleHistoricProporSplitter<IRadiantEnergyConsumer> radiantEnergySplitter;
        private readonly Dictionary<RawMaterial, HistoricRounder> reactionNumberRounders;
        private readonly HistoricRounder energyToDissipateRounder, heatEnergyToDissipateRounder, reflectedRadiantEnergyRounder, capturedForUseRadiantEnergyRounder;
        private readonly EnergyPile<RadiantEnergy> radiantEnergyToDissipatePile;
        private RadiantEnergy radiantEnergyToDissipate;
        private Mass massConvertedToEnergy;

        private readonly TextBox textBox;
        private readonly UIHorizTabPanel<IHUDElement> UITabPanel;
        private readonly IAction UITabPanelHUDPosUpdater;
        private readonly UIRectPanel<IHUDElement> infoPanel;
        private readonly TextBox infoTextBox;

        public CosmicBody(NodeState state, Func<IIndustryFacingNodeState, IIndustry?> createIndustry)
            : base(shape: new LightBlockingDisk(parameters: new ShapeParams(State: state)))
        {
            this.state = state;
            lightPolygon = new(color: state.Composition.Color());
            shape = (LightBlockingDisk)base.shape;

            links = new();

            usedLocalEnergy = ElectricalEnergy.zero;
            reactionNumberRounders = new();
            radiantEnergySplitter = new();
            energyToDissipateRounder = new();
            heatEnergyToDissipateRounder = new();
            reflectedRadiantEnergyRounder = new();
            capturedForUseRadiantEnergyRounder = new();
            radiantEnergyToDissipatePile = EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: state.LocationCounters);
            radiantEnergyToDissipate = RadiantEnergy.zero;
            massConvertedToEnergy = Mass.zero;

            textBox = new(textColor: colorConfig.almostWhiteColor);
            textBox.Shape.MinWidth = 100;
            CurWorldManager.AddWorldHUDElement
            (
                worldHUDElement: textBox,
                updateHUDPos: new TextBoxHUDPosUpdater(CosmicBody: this)
            );

            List<(string tabLabelText, ITooltip tabTooltip, IHUDElement tab)> UITabs = new();

            infoPanel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPosEnum.Left);
            UITabs.Add
            ((
                tabLabelText: "info",
                tabTooltip: new ImmutableTextTooltip(text: "Info about the planet and the industry/building on it (if such exists)"),
                tab: infoPanel
            ));
            infoTextBox = new();
            infoPanel.AddChild(child: infoTextBox);

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                tabs: UITabs
            );
            UITabPanelHUDPosUpdater = new HUDElementPosUpdater
            (
                HUDElement: UITabPanel,
                baseWorldObject: this,
                HUDElementOrigin: new(HorizPosEnum.Left, VertPosEnum.Middle),
                anchorInBaseWorldObject: new(HorizPosEnum.Right, VertPosEnum.Middle)
            );

            Industry = createIndustry(state);

            CurWorldManager.AddLightSource(lightSource: this);
            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public MyVector2 GetSpecPos(PosEnums origin)
            => Industry?.GetSpecPos(origin: origin) ?? origin.GetPosInRect(center: Position, width: 2 * state.Radius, height: 2 * state.Radius);

        public void StartConstruction(Construction.ConcreteParams constrConcreteParams)
            => Industry = constrConcreteParams.CreateIndustry();

        public void PreEnergyDistribUpdate()
            => Industry?.FrameStart();

        public void StartUpdate(EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> personFirstLinks, EnergyPile<HeatEnergy> vacuumHeatEnergyPile)
        {
#warning if the game is paused, may need to not call Industry.FrameStart() and Industry.Update() as they may fail due to division by 0 since Elapsed is 0
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

            RawMatAmounts cosmicBodyNewComposition = Algorithms.CosmicBodyNewCompositionFromNuclearFusion
            (
                curResConfig: CurResConfig,
                composition: state.Composition,
                temperature: state.Temperature,
                gravity: state.SurfaceGravity,
                duration: CurWorldManager.Elapsed,
                reactionNumberRounder: (RawMaterial rawMaterial, decimal reactionNum) =>
                {
                    if (!reactionNumberRounders.ContainsKey(rawMaterial))
                        reactionNumberRounders[rawMaterial] = new();
                    return reactionNumberRounders[rawMaterial].Round(value: reactionNum, curTime: CurWorldManager.CurTime);
                },
                nonReactingProporForUnitReactionStrengthUnitTime: CurWorldConfig.nonReactingProporForUnitReactionStrengthUnitTime
            );

            massConvertedToEnergy = state.consistsOfResPile.Amount.Mass() - cosmicBodyNewComposition.Mass();

            state.consistsOfResPile.PerformFusion(finalResAmounts: cosmicBodyNewComposition);

            state.RecalculateValues();

            (HeatEnergy heatEnergyToDissipate, radiantEnergyToDissipate) = Algorithms.EnergiesToDissipate
            (
                heatEnergy: state.ThermalBody.HeatEnergy,
                surfaceLength: state.SurfaceLength,
                emissivity: Industry?.SurfaceMatPalette?.Emissivity(temperature: state.Temperature) ?? state.Composition.Emissivity(temperature: state.Temperature),
                temperature: state.Temperature,
                duration: CurWorldManager.Elapsed,
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

            state.UpdateTemperature();

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

            Debug.Assert(state.RadiantEnergyPile.Amount.IsZero);
        }

        /// <summary>
        /// This must be called after distributing resources
        /// </summary>
        public void EndUpdate(EfficientReadOnlyDictionary<(NodeID, NodeID), Link?> resFirstLinks)
        {
            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                NodeID destinationId = resAmountsPacket.destination;
                Debug.Assert(destinationId != NodeID);

                resFirstLinks[(NodeID, destinationId)]!.TransferAllFrom(start: this, resAmountsPacket: resAmountsPacket);
            }

#warning Complete this
            infoTextBox.Text = $"""
                consists of {state.Composition}
                Mass of everything {state.LocationCounters.GetCount<AllResAmounts>().Mass()}
                Mass of planet {state.PlanetMass}
                
                """;

            textBox.Text = "";

            textBox.Text += $"T = {state.Temperature}\nM to E = {massConvertedToEnergy.valueInKg}\n";
            textBox.Text = textBox.Text.Trim();

            infoTextBox.Text += textBox.Text;
            infoTextBox.Text = infoTextBox.Text.Trim();
        }

        //public void UpdatePeople()
        //{
        //    Industry?.UpdatePeople();
        //    state.WaitingPeople.Update(updatePersonSkillsParams: null);
        //    Debug.Assert(state.LocationCounters.GetCount<NumPeople>() == state.WaitingPeople.NumPeople + (Industry?.Stats.totalNumPeople ?? NumPeople.zero));
        //    Stats = state.WaitingPeople.Stats.CombineWith(Industry?.Stats ?? RealPeopleStats.empty);
        //}

        protected sealed override void DrawPreBackground(Color otherColor, Propor otherColorPropor)
        {
            base.DrawPreBackground(otherColor, otherColorPropor);

            Industry?.BuildingImage.Draw(otherColor: otherColor, otherColorPropor: otherColorPropor);
            //Industry?.Draw(otherColor: otherColor, otherColorPropor: otherColorPropor);
        }

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
            var resArrivingHere = resAmountsPackets.ReturnAndRemove(destination: NodeID);
            if (!resArrivingHere.IsEmpty)
            {
                if (Industry is null)
                    throw new ArgumentException("Resources arrived but there is no industry to take them");
                Industry.Arrive(arrivingResPile: resArrivingHere);
            }
            
            state.waitingResAmountsPackets.TransferAllFrom(sourcePackets: resAmountsPackets);
        }

        void ILinkFacingCosmicBody.ArriveAndDeleteSource(RealPeople realPeopleSource)
            => state.WaitingPeople.TransferAllFromAndDeleteSource(realPeopleSource: realPeopleSource);

        void ILinkFacingCosmicBody.Arrive(RealPerson realPerson, RealPeople realPersonSource)
            => state.WaitingPeople.TransferFrom(realPerson: realPerson, realPersonSource: realPersonSource);

        void ILinkFacingCosmicBody.TransformAllElectricityToHeatAndTransferFrom(EnergyPile<ElectricalEnergy> source)
            => state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: source);

        void INodeAsLocalEnergyProducerAndConsumer.ProduceLocalEnergy(EnergyPile<ElectricalEnergy> destin)
            => locallyProducedEnergy = state.RadiantEnergyPile.TransformProporTo
            (
                destin: destin,
                propor: CurWorldConfig.planetTransformRadiantToElectricalEnergyPropor,
                amountToTransformRoundFunc: amount => capturedForUseRadiantEnergyRounder.Round(value: amount, curTime: CurWorldManager.CurTime)
            );

        private ILightBlockingObject CurLightCatchingObject
            => (ILightBlockingObject?)(Industry?.BuildingImage) ?? shape;

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
                    propor: Industry?.SurfaceMatPalette?.Reflectivity(temperature: state.Temperature) ?? state.Composition.Reflectivity(temperature: state.Temperature),
                    roundFunc: amount => reflectedRadiantEnergyRounder.Round
                    (
                        value: amount,
                        curTime: CurWorldManager.CurTime
                    )
                )
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
                var arcsForObjects = lightCatchingObjects.ToDictionary
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
