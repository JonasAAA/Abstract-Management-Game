using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using Game1.Collections;
using Game1.Delegates;

namespace Game1
{
    // TODO: consider making this record class, but see comment below (where operator == is commented out)
    /// <summary>
    /// Travellers take energy from the start node
    /// Travellers going to the same direction mix their heat
    /// </summary>
    [Serializable]
    public sealed class Link : WorldUIElement, IWithSpecialPositions, IWithRealPeopleStats, IWorldObject
    {
        [Serializable]
        private sealed class DirLink : IEnergyConsumer, IWithRealPeopleStats
        {
            public RealPeopleStats Stats { get; private set; }
            public HeatCapacity HeatCapacity
                => thermalBody.HeatCapacity;
            public HeatEnergy HeatEnergy
                => thermalBody.HeatEnergy;

            public readonly ILinkFacingCosmicBody startNode, endNode;

            // TODO: think about if DirLink or Link should have MassCounter
            private readonly LocationCounters locationCounters;
            // Thermal body must definately be separate for each direction so that the resources traveling to opposite sides don't exchange heat
            private readonly ThermalBody thermalBody;
            private readonly TimedPacketQueue timedPacketQueue;
            private readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
            private readonly RealPeople waitingPeople;
            private readonly Length minSafeDist;
            private readonly EnergyPile<ElectricalEnergy> allocEnergyPile;
            private Propor minSafePropor;
            private UDouble reqWattsPerKg;
            private Propor allocEnergyPropor;

            public DirLink(ILinkFacingCosmicBody startNode, ILinkFacingCosmicBody endNode, Length minSafeDist)
            {
                this.startNode = startNode;
                this.endNode = endNode;
                this.minSafeDist = minSafeDist;

                locationCounters = LocationCounters.CreateEmpty();
                thermalBody = ThermalBody.CreateEmpty(locationCounters: locationCounters);
                timedPacketQueue = new
                (
                    thermalBody: thermalBody,
                    electricalEnergySourceNodeID: EnergySourceNode.NodeID,
                    closestNodeID: endNode.NodeID
                );
                waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(thermalBody: thermalBody);
                waitingPeople = RealPeople.CreateEmpty
                (
                    thermalBody: thermalBody,
                    energyDistributor: CurWorldManager.EnergyDistributor,
                    electricalEnergySourceNodeID: EnergySourceNode.NodeID,
                    closestNodeID: endNode.NodeID,
                    isInActivityCenter: false
                );
                allocEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: locationCounters);
                allocEnergyPropor = Propor.empty;

                CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);
            }

            public void TransferAllFrom(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.TransferAllFrom(sourcePacket: resAmountsPacket);

            public void TransferAllFromAndDeleteSource(RealPeople realPeopleSource)
                => waitingPeople.TransferAllFromAndDeleteSource(realPeopleSource: realPeopleSource);

            public void TransferFrom(RealPeople realPersonSource, RealPerson realPerson)
                => waitingPeople.TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

            public AllResAmounts GetTravellingResAmounts()
            {
                Debug.Assert(locationCounters.GetCount<AllResAmounts>().Mass() == waitingResAmountsPackets.Mass + waitingPeople.Stats.totalMass + timedPacketQueue.Mass);
                Debug.Assert(locationCounters.GetCount<NumPeople>() == waitingPeople.NumPeople + timedPacketQueue.NumPeople);
                return timedPacketQueue.TotalResAmounts;
            }

            public void Update(TimeSpan travelTime, UDouble reqJoulesPerKg, Length linkLength)
            {
                if (travelTime <= TimeSpan.Zero)
                    throw new ArgumentException();
                reqWattsPerKg = reqJoulesPerKg / (UDouble)travelTime.TotalSeconds;
                minSafePropor = Propor.Create(part: minSafeDist.valueInM, whole: linkLength.valueInM) switch
                {
                    Propor propor => propor,
                    null => throw new ArgumentException()
                };

                EnergySourceNode.TransformAllElectricityToHeatAndTransferFrom(source: allocEnergyPile);

                timedPacketQueue.Update(duration: travelTime, workingPropor: allocEnergyPropor);
                var (resAmountsPackets, people) = timedPacketQueue.DonePacketsAndPeople();
                endNode.Arrive(resAmountsPackets: resAmountsPackets);
                endNode.ArriveAndDeleteSource(realPeopleSource: people);

                if ((!waitingResAmountsPackets.Empty || !waitingPeople.NumPeople.IsZero)
                    && (timedPacketQueue.Count is 0 || timedPacketQueue.LastCompletionPropor() >= minSafePropor))
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, realPeople: waitingPeople);
            }

            public void UpdatePeople()
            {
                timedPacketQueue.UpdatePeople(personalUpdate: null);
                waitingPeople.Update(updatePersonSkillsParams: null);
                Stats = timedPacketQueue.Stats.CombineWith(other: waitingPeople.Stats);
            }

            public void DrawTravelingRes()
            {
                foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                    DiskAlgos.Draw
                    (
                        center: startNode.Position + (double)complProp * (endNode.Position - startNode.Position),
                        radius: DiskAlgos.RadiusFromArea(area: resAmounts.Area().ToDouble()),
                        color: colorConfig.linkTravellerColor
                    );
            }

            EnergyPriority IEnergyConsumer.EnergyPriority
                => CurWorldConfig.linkEnergyPrior;

            NodeID IEnergyConsumer.NodeID
                => EnergySourceNode.NodeID;

            ElectricalEnergy IEnergyConsumer.ReqEnergy()
                => ReqEnergy();

            private ILinkFacingCosmicBody EnergySourceNode
                => startNode;

            private ElectricalEnergy ReqEnergy()
                => ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: MyMathHelper.RoundNonneg
                    (
                        timedPacketQueue.Mass.valueInKg * (decimal)(reqWattsPerKg * CurWorldManager.Elapsed.TotalSeconds)
                    )
                );

            void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                allocEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                allocEnergyPropor = MyMathHelper.CreatePropor(part: electricalEnergy, whole: ReqEnergy());
            }
        }

        [Serializable]
        private sealed class ShapeParams(ILinkFacingCosmicBody node1, ILinkFacingCosmicBody node2) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => node1.Position;

            public MyVector2 EndPos
                => node2.Position;

            public Length Width
                => CurWorldConfig.linkWidth;
        }

        public readonly ILinkFacingCosmicBody node1, node2;
        public UDouble JoulesPerKg { get; private set; }
        public TimeSpan TravelTime { get; private set; }
        public RealPeopleStats Stats { get; private set; }
        public HeatCapacity HeatCapacity
            => link1To2.HeatCapacity + link2To1.HeatCapacity;
        public HeatEnergy HeatEnergy
            => link1To2.HeatEnergy + link1To2.HeatEnergy;

        protected sealed override EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> Popups { get; }
        protected sealed override Color Color
            => Color.Lerp
            (
                value1: colorConfig.cheapLinkColor,
                value2: colorConfig.costlyLinkColor,
                amount: (float)(JoulesPerKg / CurWorldManager.MaxLinkJoulesPerKg)
            );

        private readonly DirLink link1To2, link2To1;
        private readonly TextBox infoTextBox;
        private IHUDElement travellingResUI;
        private readonly UIRectVertPanel<IHUDElement> linkUI;

        public Link(ILinkFacingCosmicBody node1, ILinkFacingCosmicBody node2, Length minSafeDist)
            : base
            (
                shape: new LineSegment
                (
                    worldCamera: CurWorldManager.worldCamera,
                    parameters: new ShapeParams(node1: node1, node2: node2)
                )
            )
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;

            link1To2 = new(startNode: node1, endNode: node2, minSafeDist: minSafeDist);
            link2To1 = new(startNode: node2, endNode: node1, minSafeDist: minSafeDist);

            infoTextBox = new(backgroundColor: colorConfig.UIBackgroundColor);
            travellingResUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: GetTravellingResAmounts());
            linkUI = new
            (
                childHorizPos: HorizPosEnum.Left,
                children: [infoTextBox, travellingResUI]
            );
            Popups = new List<(IHUDElement popup, IAction popupHUDPosUpdater)>()
            {
                (
                    popup: linkUI,
                    popupHUDPosUpdater: new HUDElementPosUpdater
                    (
                        HUDElement: linkUI,
                        baseWorldObject: this,
                        HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Middle),
                        anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                    )
                )
            }.ToEfficientReadOnlyCollection();

            RecalculateValuesAndGetLinkLength();
        }

        private AllResAmounts GetTravellingResAmounts()
            => link1To2.GetTravellingResAmounts() + link2To1.GetTravellingResAmounts();

        MyVector2 IWithSpecialPositions.GetSpecPos(PosEnums origin)
            => (node1.Position + node2.Position) / 2;

        public ILinkFacingCosmicBody OtherNode(ILinkFacingCosmicBody node)
        {
            if (!Contains(node))
                throw new ArgumentException();
            return node == node1 ? node2 : node1;
        }

        public bool Contains(ILinkFacingCosmicBody node)
            => node == node1 || node == node2;

        private DirLink GetDirLink(ILinkFacingCosmicBody start)
        {
            if (start == node1)
                return link1To2;
            if (start == node2)
                return link2To1;
            throw new ArgumentException();
        }

        public void TransferAllFrom(ILinkFacingCosmicBody start, ResAmountsPacket resAmountsPacket)
            => GetDirLink(start: start).TransferAllFrom(resAmountsPacket: resAmountsPacket);

        public void TransferAllFromAndDeletePeopleSource(ILinkFacingCosmicBody start, RealPeople realPeopleSource)
            => GetDirLink(start: start).TransferAllFromAndDeleteSource(realPeopleSource: realPeopleSource);

        public void TransferFrom(ILinkFacingCosmicBody start, RealPeople realPersonSource, RealPerson realPerson)
            => GetDirLink(start: start).TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

        private Length RecalculateValuesAndGetLinkLength()
        {
            Length linkLength = MyVector2.Distance(value1: node1.Position, value2: node2.Position);

            TravelTime = WorldFunctions.LinkTravelTime(linkLength: linkLength);
            JoulesPerKg = WorldFunctions.LinkJoulesPerKg
            (
                surfaceGravity1: node1.SurfaceGravity,
                surfaceGravity2: node2.SurfaceGravity,
                linkLength: linkLength
            );

            return linkLength;
        }

        public void StartUpdate()
        {
            var linkLength = RecalculateValuesAndGetLinkLength();

            link1To2.Update(travelTime: TravelTime, reqJoulesPerKg: JoulesPerKg, linkLength: linkLength);
            link2To1.Update(travelTime: TravelTime, reqJoulesPerKg: JoulesPerKg, linkLength: linkLength);
        }

        public void EndUpdate()
        {
            link1To2.UpdatePeople();
            link2To1.UpdatePeople();
            Stats = link1To2.Stats.CombineWith(other: link2To1.Stats);

            if (Active)
            {
                // Only need to update UI if it is actually visible
                infoTextBox.Text = $"Travel cost is {JoulesPerKg / CurWorldConfig.worldSecondsInGameSecond * 1_000_000_000:0.} W/MT\nTravelling resources";
                linkUI.ReplaceChild
                (
                    oldChild: ref travellingResUI,
                    newChild: ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: GetTravellingResAmounts())
                );
            }
        }

        protected sealed override void DrawChildren()
        {
            base.DrawChildren();

            link1To2.DrawTravelingRes();
            link2To1.DrawTravelingRes();
        }

        // this is commented out, otherwise the object construction fails as
        // tries to put object into HashSet before assigning node1 and node2

        //public sealed override int GetHashCode()
        //    => node1.GetHashCode() ^ node2.GetHashCode();

        //public static bool operator ==(Link link1, Link link2)
        //    => (link1.node1 == link2.node1 && link1.node2 == link2.node2) ||
        //    (link1.node1 == link2.node2 && link1.node2 == link2.node1);

        //public static bool operator !=(Link link1, Link link2)
        //    => !(link1 == link2);

        //public sealed override bool Equals(object obj)
        //{
        //    if (obj is Link other)
        //        return this == other;

        //    return false;
        //}
    }
}
