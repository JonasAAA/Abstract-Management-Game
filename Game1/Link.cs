using Game1.Delegates;
using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

namespace Game1
{
    // TODO: consider making this record class, but see comment below (where operator == is commented out)
    /// <summary>
    /// Travellers take energy from the start node
    /// </summary>
    [Serializable]
    public sealed class Link : WorldUIElement, IWithRealPeopleStats
    {
        [Serializable]
        private sealed class DirLink : IEnergyConsumer, IWithRealPeopleStats
        {
            private static readonly Texture2D diskTexture;

            static DirLink()
                => diskTexture = C.LoadTexture(name: "big disk");

            /// <summary>
            /// CURRENTLY UNUSED
            /// </summary>
            public IEvent<IDeletedListener> Deleted
                => deleted;

            public RealPeopleStats RealPeopleStats { get; private set; }

            public readonly ILinkFacingPlanet startNode, endNode;

            // TODO: think about if DirLink or Link should have MassCounter
            private readonly LocationCounters locationCounters;
            private readonly TimedPacketQueue timedPacketQueue;
            private readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
            private readonly RealPeople waitingPeople;
            private readonly UDouble minSafeDist;
            private Propor minSafePropor;
            private UDouble reqWattsPerKg;
            private Propor energyPropor;
            private readonly Event<IDeletedListener> deleted;

            public DirLink(ILinkFacingPlanet startNode, ILinkFacingPlanet endNode, UDouble minSafeDist)
            {
                this.startNode = startNode;
                this.endNode = endNode;
                this.minSafeDist = minSafeDist;

                locationCounters = LocationCounters.CreateEmpty();
                timedPacketQueue = new(locationCounters: locationCounters);
                waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(locationCounters: locationCounters);
                waitingPeople = RealPeople.CreateEmpty(locationCounters: locationCounters, energyDistributor: CurWorldManager.EnergyDistributor);
                energyPropor = Propor.empty;
                deleted = new();

                CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);
            }

            public void TransferAllFrom(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.TransferAllFrom(sourcePacket: resAmountsPacket);

            public void TransferAllFrom(RealPeople realPeople)
                => waitingPeople.TransferAllFrom(realPeopleSource: realPeople);

            public void TransferFrom(RealPeople realPersonSource, RealPerson realPerson)
                => waitingPeople.TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

            public ulong GetTravellingAmount()
            {
                Debug.Assert(locationCounters.Mass == waitingResAmountsPackets.Mass + waitingPeople.RealPeopleStats.totalMass + timedPacketQueue.Mass);
                Debug.Assert(locationCounters.NumPeople == waitingPeople.NumPeople + timedPacketQueue.NumPeople);
                return CurWorldManager.Overlay.SwitchExpression
                (
                    singleResCase: resInd => timedPacketQueue.TotalResAmounts[resInd],
                    allResCase: () => timedPacketQueue.Mass.valueInKg,
                    peopleCase: () => locationCounters.NumPeople.value,
                    powerCase: () => throw new InvalidOperationException()
                );
            }

            public void Update(TimeSpan travelTime, UDouble reqJoulesPerKg, UDouble linkLength)
            {
                if (travelTime <= TimeSpan.Zero)
                    throw new ArgumentException();
                reqWattsPerKg = reqJoulesPerKg / (UDouble)travelTime.TotalSeconds;
                minSafePropor = Propor.Create(part: minSafeDist, whole: linkLength) switch
                {
                    Propor propor => propor,
                    null => throw new ArgumentException()
                };

                timedPacketQueue.Update(duration: travelTime, workingPropor: energyPropor);
                var (resAmountsPackets, people) = timedPacketQueue.DonePacketsAndPeople();
                endNode.Arrive(resAmountsPackets: resAmountsPackets);
                endNode.Arrive(realPeople: people);

                if ((!waitingResAmountsPackets.Empty || !waitingPeople.NumPeople.IsZero)
                    && (timedPacketQueue.Count is 0 || timedPacketQueue.LastCompletionPropor() >= minSafePropor))
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, realPeople: waitingPeople);
            }

            public void UpdatePeople()
            {
                RealPerson.UpdateLocationParams personUpdateParams = new(LastNodeID: startNode.NodeID, ClosestNodeID: endNode.NodeID);
                timedPacketQueue.UpdatePeople(updateLocationParams: personUpdateParams, personalUpdate: null);
                waitingPeople.Update(updateLocationParams: personUpdateParams, personalUpdateSkillsParams: null);
                RealPeopleStats = timedPacketQueue.RealPeopleStats.CombineWith(other: waitingPeople.RealPeopleStats);
            }

            public void DrawTravelingRes()
            {
                // temporary
                CurWorldManager.Overlay.SwitchStatement
                (
                    singleResCase: resInd =>
                    {
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts[resInd]);
                    },
                    allResCase: () =>
                    {
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts.Mass().valueInKg);
                    },
                    powerCase: () => { },
                    peopleCase: () =>
                    {
                        foreach (var (complProp, _, numPeople) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: numPeople.value);
                    }
                );

                void DrawDisk(Propor complProp, UDouble size)
                    => C.Draw
                    (
                        texture: diskTexture,
                        position: startNode.Position + (double)complProp * (endNode.Position - startNode.Position),
                        color: colorConfig.linkTravellerColor,
                        rotation: 0,
                        origin: new MyVector2(diskTexture.Width * .5, diskTexture.Height * .5),
                        scale: MyMathHelper.Sqrt(size) * 2 / (UDouble)diskTexture.Width
                    );
            }

            EnergyPriority IEnergyConsumer.EnergyPriority
                => CurWorldConfig.linkEnergyPrior;

            NodeID IEnergyConsumer.NodeID
                => startNode.NodeID;

            ElectricalEnergy IEnergyConsumer.ReqEnergy()
                => throw new NotImplementedException();
            //=> timedPacketQueue.Mass.valueInKg * reqWattsPerKg;

            void IEnergyConsumer.ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
                => throw new NotImplementedException();
                //=> this.energyPropor = energyPropor;
        }

        [Serializable]
        private readonly record struct ShapeParams(ILinkFacingPlanet Node1, ILinkFacingPlanet Node2) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => Node1.Position;

            public MyVector2 EndPos
                => Node2.Position;

            public UDouble Width
                => CurWorldConfig.linkWidth;
        }

        public readonly ILinkFacingPlanet node1, node2;
        public UDouble JoulesPerKg { get; private set; }
        public TimeSpan TravelTime { get; private set; }
        public RealPeopleStats RealPeopleStats { get; private set; }

        private readonly DirLink link1To2, link2To1;
        private readonly TextBox infoTextBox;

        public Link(ILinkFacingPlanet node1, ILinkFacingPlanet node2, UDouble minSafeDist)
            : base
            (
                shape: new LineSegment
                (
                    parameters: new ShapeParams(Node1: node1, Node2: node2)
                ),
                activeColor: Color.White,
                inactiveColor: Color.Green,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;

            link1To2 = new(startNode: node1, endNode: node2, minSafeDist: minSafeDist);
            link2To1 = new(startNode: node2, endNode: node1, minSafeDist: minSafeDist);

            infoTextBox = new(backgroundColor: Color.White);
            SetPopup(HUDElement: infoTextBox, overlays: IOverlay.all);
        }

        public ILinkFacingPlanet OtherNode(ILinkFacingPlanet node)
        {
            if (!Contains(node))
                throw new ArgumentException();
            return node == node1 ? node2 : node1;
        }

        public bool Contains(ILinkFacingPlanet node)
            => node == node1 || node == node2;

        private DirLink GetDirLink(ILinkFacingPlanet start)
        {
            if (start == node1)
                return link1To2;
            if (start == node2)
                return link2To1;
            throw new ArgumentException();
        }

        public void TransferAllFrom(ILinkFacingPlanet start, ResAmountsPacket resAmountsPacket)
            => GetDirLink(start: start).TransferAllFrom(resAmountsPacket: resAmountsPacket);

        public void TransferAllFrom(ILinkFacingPlanet start, RealPeople realPeople)
            => GetDirLink(start: start).TransferAllFrom(realPeople: realPeople);

        public void TransferFrom(ILinkFacingPlanet start, RealPeople realPersonSource, RealPerson realPerson)
            => GetDirLink(start: start).TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

        public void Update()
        {
            UDouble linkLength = MyVector2.Distance(value1: node1.Position, value2: node2.Position);

            TravelTime = WorldFunctions.LinkTravelTime(linkLength: linkLength);
            JoulesPerKg = WorldFunctions.LinkJoulesPerKg
            (
                surfaceGravity1: node1.SurfaceGravity,
                surfaceGravity2: node2.SurfaceGravity,
                linkLength: linkLength
            );

            link1To2.Update(travelTime: TravelTime, reqJoulesPerKg: JoulesPerKg, linkLength: linkLength);
            link2To1.Update(travelTime: TravelTime, reqJoulesPerKg: JoulesPerKg, linkLength: linkLength);

            inactiveColor = C.ColorFromRGB(rgb: 0x003654);
            // TODO(color): turn activeColor and inactiveColor into abstract properties
            //inactiveColor = Color.Lerp
            //(
            //    value1: Color.White,
            //    value2: Color.Green,
            //    amount: CurWorldManager.Overlay switch
            //    {
            //        IPeopleOverlay => (float)(TravelTime / CurWorldManager.MaxLinkTravelTime),
            //        _ => (float)(JoulesPerKg / CurWorldManager.MaxLinkJoulesPerKg)
            //    }
            //);
        }

        public void UpdatePeople()
        {
            link1To2.UpdatePeople();
            link2To1.UpdatePeople();
            RealPeopleStats = link1To2.RealPeopleStats.CombineWith(other: link2To1.RealPeopleStats);

            if (CurWorldManager.Overlay is IPowerOverlay)
                return;

            // TODO: It may be more appropriate for link1To2.GetTravellingAmount() to return a dictionary from Overlay cases to amounts
            // in order to not have two switch statements mirroring each other
            ulong travellingAmount = link1To2.GetTravellingAmount() + link2To1.GetTravellingAmount();

            infoTextBox.Text = $"Travel cost is {JoulesPerKg:0.000} J/Kg\n" + CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd => $"{travellingAmount} of {CurWorldManager.Overlay} is travelling",
                allResCase: () => $"{travellingAmount} kg of resources are travelling",
                peopleCase: () => $"travelling people stats:\n{RealPeopleStats}",
                powerCase: () => ""
            );
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();

            link1To2.DrawTravelingRes();
            link2To1.DrawTravelingRes();
        }

        // this is commented out, otherwise the object construction fails as
        // tries to put object into HashSet before assigning node1 and node2

        //public override int GetHashCode()
        //    => node1.GetHashCode() ^ node2.GetHashCode();

        //public static bool operator ==(Link link1, Link link2)
        //    => (link1.node1 == link2.node1 && link1.node2 == link2.node2) ||
        //    (link1.node1 == link2.node2 && link1.node2 == link2.node1);

        //public static bool operator !=(Link link1, Link link2)
        //    => !(link1 == link2);

        //public override bool Equals(object obj)
        //{
        //    if (obj is Link other)
        //        return this == other;

        //    return false;
        //}
    }
}
